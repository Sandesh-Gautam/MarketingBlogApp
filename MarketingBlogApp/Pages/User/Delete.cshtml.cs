using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MarketingBlogApp.Pages.User
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
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

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
