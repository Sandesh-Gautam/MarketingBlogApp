using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<BlogPost> BlogPosts { get; set; }
        public IList<Category> CategoryOptions { get; set; }
        public string SearchString { get; set; }
        public string SearchCategory { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public async Task<IActionResult> OnGetAsync(string searchString, string searchCategory, string sortBy = "date", string sortOrder = "desc", int page = 1)
        {
            var userId = _userManager.GetUserId(User);

            try
            {
                if (User.Identity.IsAuthenticated && Request.Cookies["WarningsShown"] == null)
                {
                    var unresolvedWarnings = await GetUnresolvedWarningsAsync();

                    if (unresolvedWarnings.Any())
                    {
                        TempData["Warnings"] = unresolvedWarnings;
                    }

                    Response.Cookies.Append("WarningsShown", "true", new CookieOptions
                    {
                        Expires = DateTime.Now.AddMinutes(30)
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework or service)
                Console.WriteLine($"Error checking warnings: {ex.Message}");
            }

            SearchString = searchString;
            SearchCategory = searchCategory;
            SortBy = sortBy;
            SortOrder = sortOrder;

            IQueryable<BlogPost> blogPostsIQ = _context.BlogPosts
                .Include(b => b.BlogPostCategories).ThenInclude(bc => bc.Category)
                .Include(b => b.Likes)
                .Include(b => b.Author)
                .Include(b => b.Comments).ThenInclude(c => c.User)
                .Where(b => b.IsVisible); // Only fetch posts that are marked as visible

            if (!string.IsNullOrEmpty(SearchString))
            {
                blogPostsIQ = blogPostsIQ.Where(b => b.Title.Contains(SearchString) || b.Content.Contains(SearchString));
            }

            if (!string.IsNullOrEmpty(SearchCategory))
            {
                blogPostsIQ = blogPostsIQ.Where(b => b.BlogPostCategories.Any(bc => bc.Category.Name == SearchCategory));
            }

            blogPostsIQ = SortBy switch
            {
                "likes" => SortOrder == "asc" ? blogPostsIQ.OrderBy(b => b.Likes.Count) : blogPostsIQ.OrderByDescending(b => b.Likes.Count),
                _ => SortOrder == "asc" ? blogPostsIQ.OrderBy(b => b.PublishedDate) : blogPostsIQ.OrderByDescending(b => b.PublishedDate),
            };

            int totalCount = await blogPostsIQ.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            BlogPosts = await blogPostsIQ
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            CategoryOptions = await _context.Categories.ToListAsync();
            CurrentPage = page;

            return Page();
        }


        private async Task<List<string>> GetUnresolvedWarningsAsync()
        {
            var userId = _userManager.GetUserId(User);
            var warnings = await _context.Warnings
                .Where(w => w.UserId == userId && !w.IsResolved)
                .Select(w => w.Reason)
                .ToListAsync();

            return warnings;
        }

        public async Task<IActionResult> OnPostToggleLikeAsync(int postId)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var like = await _context.Likes.FirstOrDefaultAsync(l => l.BlogPostId == postId && l.UserId == userId);

            if (like == null)
            {
                _context.Likes.Add(new Like { BlogPostId = postId, UserId = userId });
            }
            else
            {
                _context.Likes.Remove(like);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddCommentAsync(int postId, string commentContent)
        {
            if (string.IsNullOrWhiteSpace(commentContent))
            {
                ModelState.AddModelError("commentContent", "Comment cannot be empty.");
                return RedirectToPage();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                // Handle the case where the user is not authenticated or userId is null
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var comment = new Comment
            {
                BlogPostId = postId,
                UserId = userId,
                Content = commentContent,
                CommentedDate = DateTime.Now,
                IsVisible = true
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(commentId);

                if (comment != null)
                {
                    comment.IsVisible = false;
                    _context.Comments.Update(comment);
                    await _context.SaveChangesAsync();

                    var warning = new Warning
                    {
                        UserId = comment.UserId,
                        Reason = $"Your comment with ID {commentId} was deleted.",
                        DateIssued = DateTime.Now,
                        IsResolved = false,
                        CommentId = commentId
                    };

                    _context.Warnings.Add(warning);
                    await _context.SaveChangesAsync();
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logging framework here)
                Console.WriteLine($"Error deleting comment: {ex.Message}");
                // Optionally, you could also show an error message to the user
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the comment.");
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeletePostAsync(int postId, string reason, IFormFile proofImage)
        {
            try
            {
                var post = await _context.BlogPosts.FindAsync(postId);
                var userId = _userManager.GetUserId(User);

                if (post != null)
                {
                    // Set the specific post to be invisible
                    post.IsVisible = false;
                    _context.BlogPosts.Update(post);
                    await _context.SaveChangesAsync();

                    // Save the proof image if provided
                    var proofImageUrl = await SaveProofImageAsync(proofImage);

                    // Issue a warning to the user for this specific post
                    var warning = new Warning
                    {
                        UserId = post.AuthorId,
                        Reason = $"Your post with ID {postId} was deleted: {reason}",
                        DateIssued = DateTime.Now,
                        IsResolved = false,
                        BlogPostId = postId
                    };

                    _context.Warnings.Add(warning);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logging framework here)
                Console.WriteLine($"Error deleting post: {ex.Message}");
                // Optionally, you could also show an error message to the user
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the post.");
                return RedirectToPage();
            }

            return RedirectToPage();
        }


        private async Task<string> SaveProofImageAsync(IFormFile proofImage)
        {
            if (proofImage == null || proofImage.Length == 0)
            {
                return null;
            }

            var filePath = Path.Combine("wwwroot/uploads", proofImage.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await proofImage.CopyToAsync(stream);
            }

            return "/uploads/" + proofImage.FileName;
        }

        private async Task LogUserActivity(string userId, string activityType)
        {
            var activity = new UserActivity
            {
                UserId = userId,
                ActivityType = activityType,
                ActivityDate = DateTime.Now
            };

            _context.UserActivities.Add(activity);
            await _context.SaveChangesAsync();
        }
    }
}
