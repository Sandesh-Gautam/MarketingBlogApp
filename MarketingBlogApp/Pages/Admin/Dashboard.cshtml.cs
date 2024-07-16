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

        public int TotalPosts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalVisits { get; set; }
        public int TotalUsers { get; set; }
        public int TotalManagers { get; set; }

        public IList<ManagerAction> ManagerActions { get; set; }
        public IList<Warning> Warnings { get; set; }

        public List<ChartData> PostsOverTime { get; set; }
        public List<ChartData> UserGrowth { get; set; }
        public List<ChartData> VisitsOverTime { get; set; }

        public DashboardModel(ILogger<DashboardModel> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            TotalPosts = await _context.BlogPosts.CountAsync();
            TotalCategories = await _context.Categories.CountAsync();
            TotalVisits = await _context.UserActivities.CountAsync(ua => ua.ActivityType == "Page Visit");
            TotalUsers = await _userManager.Users.CountAsync();
            TotalManagers = (await _userManager.GetUsersInRoleAsync("Manager")).Count;

            ManagerActions = await _context.ManagerActions
                .Include(ma => ma.Manager)
                .ToListAsync();

            Warnings = await _context.Warnings
                .Include(w => w.User)
                .ToListAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    ActivityType = "Viewed Dashboard Page",
                    ActivityDate = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }

            PostsOverTime = await _context.BlogPosts
                .GroupBy(p => new { p.PublishedDate.Year, p.PublishedDate.Month })
                .Select(g => new ChartData
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .ToListAsync();

            UserGrowth = await _userManager.Users
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new ChartData
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .ToListAsync();

            VisitsOverTime = await _context.UserActivities
                .Where(ua => ua.ActivityType == "Page Visit")
                .GroupBy(ua => new { ua.ActivityDate.Year, ua.ActivityDate.Month })
                .Select(g => new ChartData
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .ToListAsync();
        }
    }

    public class ChartData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
