using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            SearchString = searchString;
            SearchCategory = searchCategory;
            SortBy = sortBy;
            SortOrder = sortOrder;

            IQueryable<BlogPost> blogPostsIQ = _context.BlogPosts
                .Include(b => b.BlogPostCategories).ThenInclude(bc => bc.Category)
                .Include(b => b.Likes)
                .Include(b => b.Author)
                .Include(b => b.Comments).ThenInclude(c => c.User);

            if (!string.IsNullOrEmpty(SearchString))
            {
                blogPostsIQ = blogPostsIQ.Where(b => b.Title.Contains(SearchString) || b.Content.Contains(SearchString));
            }

            if (!string.IsNullOrEmpty(SearchCategory))
            {
                blogPostsIQ = blogPostsIQ.Where(b => b.BlogPostCategories.Any(bc => bc.Category.Name == SearchCategory));
            }

            // Apply sorting
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

        public async Task<IActionResult> OnPostDeletePostAsync(int postId)
        {
            var post = await _context.BlogPosts.FindAsync(postId);

            if (post != null)
            {
                _context.BlogPosts.Remove(post);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}


