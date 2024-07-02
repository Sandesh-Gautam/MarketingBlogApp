using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MarketingBlogApp.Pages.User
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

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            BlogPosts = await _context.BlogPosts
                .Where(bp => bp.AuthorId == user.Id)
                .Include(bp => bp.BlogPostCategories)
                    .ThenInclude(bc => bc.Category)
                .Include(bp => bp.Comments)
                    .ThenInclude(c => c.User)
                .Include(bp => bp.Likes)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddCommentAsync(int postId, string commentContent)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var post = await _context.BlogPosts
                .Include(bp => bp.Comments)
                .FirstOrDefaultAsync(bp => bp.Id == postId);

            if (post == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                Content = commentContent,
                CommentedDate = DateTime.Now,
                UserId = user.Id,
                BlogPostId = postId
            };

            post.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleLikeAsync(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var post = await _context.BlogPosts
                .Include(bp => bp.Likes)
                .FirstOrDefaultAsync(bp => bp.Id == postId);

            if (post == null)
            {
                return NotFound();
            }

            var like = post.Likes.FirstOrDefault(l => l.UserId == user.Id);
            if (like == null)
            {
                like = new Like
                {
                    BlogPostId = postId,
                    UserId = user.Id
                };
                post.Likes.Add(like);
            }
            else
            {
                post.Likes.Remove(like);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
