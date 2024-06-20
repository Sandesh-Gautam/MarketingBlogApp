using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class EnableModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public EnableModel(UserManager<ApplicationUser> userManager,ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public ApplicationUser User { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            User = await _userManager.FindByIdAsync(id);
            if (User != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = User.Id,
                    Activity = "Viewed Dashboard Page",
                    Timestamp = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }
            if (User == null || !User.IsDisabled)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.IsDisabled)
            {
                return NotFound();
            }

            user.IsDisabled = false;
            var result = await _userManager.UpdateAsync(user);
            if (user != null)
            {
                var enableUsers = new UserActivity
                {
                    UserId = user.Id,
                    Activity = "Deleted a User",
                    Timestamp = DateTime.Now
                };
                _context.UserActivities.Add(enableUsers);
                await _context.SaveChangesAsync();
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to enable user.");
                return Page();
            }

            return RedirectToPage("./DisabledUsers");
        }
    }
}
