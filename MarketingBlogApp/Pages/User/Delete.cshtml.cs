using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MarketingBlogApp.Pages.User
{

    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public BlogPost BlogPost { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            BlogPost = await _context.BlogPosts
                .Include(bp => bp.Comments)
                .Include(bp => bp.Likes)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (BlogPost == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            BlogPost = await _context.BlogPosts
                .Include(bp => bp.Comments)
                .Include(bp => bp.Likes)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (BlogPost == null)
            {
                return NotFound();
            }

            // Remove related Likes
            _context.Likes.RemoveRange(BlogPost.Likes);

            // Remove related Comments
            _context.Comments.RemoveRange(BlogPost.Comments);

            // Remove the BlogPost itself
            _context.BlogPosts.Remove(BlogPost);

            // Log the activity for deleting a blog post
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    ActivityType = "Deleted a blog post",
                    ActivityDate = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
