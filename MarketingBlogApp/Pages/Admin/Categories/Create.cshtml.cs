using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Category Category { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Category.BlogPostCategories");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Categories.Add(Category);
            await _context.SaveChangesAsync();

            // Log the activity for creating a category
            var user = await _userManager.GetUserAsync(User);
            var userActivity = new UserActivity
            {
                UserId = user.Id,
                ActivityType = "Created a category",
                ActivityDate = DateTime.Now
            };
            _context.UserActivities.Add(userActivity);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
