using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketingBlogApp.Models;
using MarketingBlogApp.Data;

namespace MarketingBlogApp.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class ActivityModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ActivityModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<UserActivityViewModel> UserActivity { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchUsername { get; set; }

        public async Task OnGetAsync(int? pageIndex)
        {
            const int pageSize = 40; // Number of activities per page

            // Apply filtering logic as needed
            var userActivitiesQuery = _context.UserActivities
                .OrderByDescending(a => a.ActivityDate)
                .Join(
                    _context.Users,
                    ua => ua.UserId,
                    u => u.Id,
                    (ua, u) => new { UserActivity = ua, UserName = u.UserName }
                )
                .Select(ua => new UserActivityViewModel
                {
                    UserName = ua.UserName,
                    ActivityType = ua.UserActivity.ActivityType,
                    ActivityDate = ua.UserActivity.ActivityDate
                })
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchUsername))
            {
                userActivitiesQuery = userActivitiesQuery
                    .Where(a => a.UserName.Contains(SearchUsername));
            }

            // Count total items matching the query
            var totalItems = await userActivitiesQuery.CountAsync();

            // Calculate the number of pages
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Ensure pageIndex is within valid range
            pageIndex ??= 1;
            pageIndex = Math.Max(1, Math.Min(pageIndex.Value, totalPages));

            // Calculate skip amount based on pageIndex and pageSize
            var skipAmount = (pageIndex.Value - 1) * pageSize;

            // Retrieve paged activities
            UserActivity = await userActivitiesQuery
                .Skip(skipAmount)
                .Take(pageSize)
                .ToListAsync();

            // Set additional view data
            ViewData["PageIndex"] = pageIndex.Value;
            ViewData["TotalPages"] = totalPages;
        }
    }

    public class UserActivityViewModel
    {
        public string UserName { get; set; }
        public string ActivityType { get; set; }
        public DateTime ActivityDate { get; set; }
    }
}
