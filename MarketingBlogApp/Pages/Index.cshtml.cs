using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages
{
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

            if (!string.IsNullOrEmpty(userId))
            {
                await LogUserActivity(userId, "Viewed Home Page");
            }

            SearchString = searchString;
            SearchCategory = searchCategory;
            SortBy = sortBy;
            SortOrder = sortOrder;

            // Get all unresolved warnings
            var unresolvedWarnings = await _context.Warnings
                .Where(w => !w.IsResolved)
                .Select(w => w.UserId)
                .ToListAsync();

            IQueryable<BlogPost> blogPostsIQ = _context.BlogPosts
                .Include(b => b.BlogPostCategories).ThenInclude(bc => bc.Category)
                .Include(b => b.Likes)
                .Include(b => b.Author)
                .Include(b => b.Comments).ThenInclude(c => c.User)
                .Where(b => !unresolvedWarnings.Contains(b.AuthorId));

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
            TotalPages = (int)System.Math.Ceiling(totalCount / (double)PageSize);

            BlogPosts = await blogPostsIQ
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            CategoryOptions = await _context.Categories.ToListAsync();
            CurrentPage = page;

            return Page();
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
                CommentedDate = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePostAsync(int postId, string reason, IFormFile proofImage)
        {
            var post = await _context.BlogPosts.FindAsync(postId);
            var userId = _userManager.GetUserId(User);

            if (post != null)
            {
                post.IsVisible = false;
                _context.BlogPosts.Update(post);
                await _context.SaveChangesAsync();

                // Save the proof image
                var proofImageUrl = await SaveProofImageAsync(proofImage);

                // Add deletion reason
                var deletionReason = new DeletionReason
                {
                    UserId = post.AuthorId,
                    Reason = reason,
                    ProofImageUrl = proofImageUrl,
                    DateIssued = DateTime.Now,
                    IsResolved = false
                };

                _context.DeletionReasons.Add(deletionReason);
                await _context.SaveChangesAsync();

                // Add a warning
                var warning = new Warning
                {
                    UserId = post.AuthorId,
                    Reason = "Your post was deleted: " + reason,
                    DateIssued = DateTime.Now,
                    IsResolved = false
                };

                _context.Warnings.Add(warning);
                await _context.SaveChangesAsync();
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
