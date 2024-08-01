using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            IQueryable<ApplicationUser> query = _userManager.Users.Where(u => u.IsDisabled);

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(u => u.UserName.Contains(SearchQuery) || u.Email.Contains(SearchQuery));
            }

            if (StartDate.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= EndDate.Value);
            }

            int pageSize = 10; // Number of items per page
            DisabledUsers = await query.Skip((PageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);

            // Log user activity
            await LogUserActivity("Viewed Disabled Users");
        }

        public async Task<IActionResult> OnPostEnableAsync(string id)
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
                    await LogUserActivity($"Failed to enable {user.UserName} due to unresolved warnings.");
                    return Page();
                }
            }

            // If no unresolved warnings or enough time has passed, enable the user
            user.IsDisabled = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to enable user.");
                await LogUserActivity($"Failed to enable {user.UserName}.");
                return Page();
            }

            await LogUserActivity($"Enabled {user.UserName}.");
            return RedirectToPage("./DisabledUsers"); // Redirect to the same page
        }

        private async Task LogUserActivity(string activityType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    ActivityType = activityType,
                    ActivityDate = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
