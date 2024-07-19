using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MarketingBlogApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardModel(ILogger<DashboardModel> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public int TotalPosts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalVisits { get; set; }
        public int TotalUsers { get; set; }

        public List<ChartData> VisitsOverTime { get; set; }
        public List<ChartData> PostsOverTime { get; set; }
        public List<ChartData> UserGrowth { get; set; }

        public async Task OnGetAsync()
        {
            TotalPosts = await _context.BlogPosts.CountAsync();
            TotalCategories = await _context.Categories.CountAsync();
            TotalVisits = await _context.UserActivities.CountAsync(ua => ua.ActivityType == "Page Visit");
            TotalUsers = await _userManager.Users.CountAsync();

            VisitsOverTime = await _context.UserActivities
                .Where(ua => ua.ActivityType == "Page Visit")
                .GroupBy(ua => ua.ActivityDate.Date)
                .Select(g => new ChartData { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            PostsOverTime = await _context.UserActivities
                .Where(ua => ua.ActivityType == "Created a blog post")
                .GroupBy(ua => ua.ActivityDate.Date)
                .Select(g => new ChartData { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            UserGrowth = await _context.UserActivities
                .Where(ua => ua.ActivityType == "Registered")
                .GroupBy(ua => ua.ActivityDate.Date)
                .Select(g => new ChartData { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            LogUserActivity("Dashboard viewed");
        }

        private void LogUserActivity(string activity)
        {
            var userActivity = new UserActivity
            {
                UserId = _userManager.GetUserId(User),
                ActivityType = activity,
                ActivityDate = DateTime.Now
            };
            _context.UserActivities.Add(userActivity);
            _context.SaveChanges();
        }
    }

    public class ChartData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
