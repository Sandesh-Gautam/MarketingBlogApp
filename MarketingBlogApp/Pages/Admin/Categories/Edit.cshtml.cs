using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;

namespace MarketingBlogApp.Pages.Admin.Categories
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Category Category { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _context.Categories.FindAsync(id);

            if (Category == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Category.BlogPostCategories");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Ensure the Id property is correctly set
            var categoryToUpdate = await _context.Categories.FindAsync(Category.Id);
            if (categoryToUpdate == null)
            {
                return NotFound();
            }

            categoryToUpdate.Name = Category.Name;

            try
            {
                await _context.SaveChangesAsync();

                // Log the activity for editing a category
                var user = await _userManager.GetUserAsync(User);
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    ActivityType = "Edited a category",
                    ActivityDate = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(Category.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
