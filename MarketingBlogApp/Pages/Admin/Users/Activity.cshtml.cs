using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using MarketingBlogApp.Data;

namespace MarketingBlogApp.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class ActivityModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ActivityModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IList<UserActivityViewModel> UserActivity { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SearchUsername { get; set; }

        public async Task OnGetAsync()
        {
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

            UserActivity = await userActivitiesQuery.ToListAsync();
        }
    }

    public class UserActivityViewModel
    {
        public string UserName { get; set; }
        public string ActivityType { get; set; }
        public DateTime ActivityDate { get; set; }
    }
}
