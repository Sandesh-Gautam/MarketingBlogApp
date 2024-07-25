using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin.Categories
{
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

            // Log the activity for deleting a category
            var user = await _userManager.GetUserAsync(User);
            var userActivity = new UserActivity
            {
                UserId = user.Id,
                ActivityType = "Deleted a category",
                ActivityDate = DateTime.Now
            };
            _context.UserActivities.Add(userActivity);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
