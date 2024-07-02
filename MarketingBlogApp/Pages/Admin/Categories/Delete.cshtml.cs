using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Category Category { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _context.Categories.FindAsync(id);

            if (Category == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Category = await _context.Categories.FindAsync(id);

            if (Category == null)
            {
                return NotFound();
            }

            // Check if there are any associated blog posts
            var associatedBlogPosts = _context.BlogPostCategories.Any(bc => bc.CategoryId == id);
            if (associatedBlogPosts)
            {
                ErrorMessage = "Cannot delete category because it is associated with one or more blog posts.";
                return Page();
            }

            _context.Categories.Remove(Category);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
