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
    public class DisabledModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public readonly ApplicationDbContext _context;

        public DisabledModel(UserManager<ApplicationUser> userManager,ApplicationDbContext context)
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
            if (User != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = User.Id,
                    Activity = "Viewed Delete Page",
                    Timestamp = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsDisabled = true; // Disable the user
            var result = await _userManager.UpdateAsync(user);
            if (user != null)
            {
                var disableUsers = new UserActivity
                {
                    UserId = user.Id,
                    Activity = "Disabled a User",
                    Timestamp = DateTime.Now
                };
                _context.UserActivities.Add(disableUsers);
                await _context.SaveChangesAsync();
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to disable user.");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
