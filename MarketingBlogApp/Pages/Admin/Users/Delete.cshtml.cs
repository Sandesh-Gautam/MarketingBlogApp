using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DeleteModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public ApplicationUser User { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            User = await _userManager.FindByIdAsync(id);
            if (User == null)
            {
                return NotFound();
            }

            var userActivity = new UserActivity
            {
                UserId = User.Id,
                Activity = "Viewed Delete Page",
                Timestamp = DateTime.Now
            };
            _context.UserActivities.Add(userActivity);
            await _context.SaveChangesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Log the activity before deleting the user
            var deleteUserActivity = new UserActivity
            {
                UserId = user.Id,
                Activity = "Deleted a User",
                Timestamp = DateTime.Now
            };
            _context.UserActivities.Add(deleteUserActivity);
            await _context.SaveChangesAsync();

            // Delete related UserActivity records
            var userActivities = _context.UserActivities.Where(ua => ua.UserId == id);
            _context.UserActivities.RemoveRange(userActivities);

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to delete user.");
                return Page();
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
