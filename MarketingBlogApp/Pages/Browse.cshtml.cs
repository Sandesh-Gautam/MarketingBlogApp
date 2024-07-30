using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MarketingBlogApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MarketingBlogApp.Pages
{
    [Authorize]
    public class BrowseModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BrowseModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<ApplicationUser> TopContributors { get; set; }
        public List<BlogPost> TrendingPosts { get; set; }
        public List<BlogPost> PopularPosts { get; set; }

        public async Task OnGetAsync()
        {
            // Get top 10 contributors with at least 1 post
            TopContributors = await _context.Users
                .Include(u => u.BlogPosts)
                .Where(u => u.BlogPosts.Count > 0)
                .OrderByDescending(u => u.BlogPosts.Count)
                .Take(10)
                .ToListAsync();

            // Get trending posts (e.g., posts with the most comments in the last week)
            var oneWeekAgo = DateTime.Now.AddDays(-7);
            TrendingPosts = await _context.BlogPosts
                .Where(bp => bp.Comments.Any(c => c.CommentedDate >= oneWeekAgo))
                .OrderByDescending(bp => bp.Comments.Count(c => c.CommentedDate >= oneWeekAgo))
                .Take(10)
                .Include(bp => bp.Author)
                .Include(bp => bp.Comments).ThenInclude(c => c.User)
                .Include(bp => bp.Likes)
                .ToListAsync();

            // Get popular posts (e.g., posts with the most likes and at least 1 like)
            PopularPosts = await _context.BlogPosts
                .Where(bp => bp.Likes.Count > 0)
                .OrderByDescending(bp => bp.Likes.Count)
                .Take(10)
                .Include(bp => bp.Author)
                .Include(bp => bp.Comments).ThenInclude(c => c.User)
                .Include(bp => bp.Likes)
                .ToListAsync();
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
    }
}
