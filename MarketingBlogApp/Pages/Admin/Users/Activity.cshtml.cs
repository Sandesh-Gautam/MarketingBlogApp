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

namespace MarketingBlogApp.Pages.Admin
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
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchUsername))
            {
                userActivitiesQuery = userActivitiesQuery
                    .Where(a => a.User.UserName.Contains(SearchUsername));
            }

            var userActivities = await userActivitiesQuery.ToListAsync();

            // Convert UserActivity to UserActivityViewModel
            UserActivity = userActivities.Select(a => new UserActivityViewModel
            {
                UserName = a.User.UserName,
                Activity = a.Activity,
                Timestamp = a.Timestamp
            }).ToList();
        }
    }

    public class UserActivityViewModel
    {
        public string UserName { get; set; }
        public string Activity { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
