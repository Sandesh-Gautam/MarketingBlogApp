using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace MarketingBlogApp.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class DisabledUsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DisabledUsersModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IList<ApplicationUser> DisabledUsers { get; set; }

        public async Task OnGetAsync()
        {
            DisabledUsers = await _userManager.Users.Where(u => u.IsDisabled).ToListAsync();
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    ActivityType = "Viewed Disabled Users",
                    ActivityDate = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var unresolvedWarnings = await _context.Warnings
                .Where(w => w.UserId == user.Id && !w.IsResolved)
                .ToListAsync();

            if (unresolvedWarnings.Count >= 3)
            {
                var lastWarning = unresolvedWarnings.OrderByDescending(w => w.DateIssued).First();
                var disableUntil = lastWarning.DateIssued.AddDays(7);
                var remainingDays = (disableUntil - DateTime.Now).Days;

                if (remainingDays > 0)
                {
                    ModelState.AddModelError("", $"The user has been disabled for 1 week due to unresolved warnings. {remainingDays} days left before enabling.");
                    return Page();
                }
                else
                {
                    user.IsDisabled = false;
                    var result = await _userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", "Failed to enable user.");
                        return Page();
                    }

                    return RedirectToPage("./Index");
                }
            }
            else
            {
                user.IsDisabled = false;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to enable user.");
                    return Page();
                }

                return RedirectToPage("./Index");
            }
        }
    }
}
