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
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IList<ApplicationUser> Users { get; set; }
        public Dictionary<string, IList<string>> UserRoles { get; set; }

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
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    ActivityType = "Viewed Users",
                    ActivityDate = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(u => u.FirstName.Contains(SearchQuery) ||
                                         u.LastName.Contains(SearchQuery) ||
                                         u.Email.Contains(SearchQuery) ||
                                         u.UserName.Contains(SearchQuery));
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
            TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);

            Users = await query.Skip((PageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            UserRoles = new Dictionary<string, IList<string>>();

            foreach (var obj in Users)
            {
                var roles = await _userManager.GetRolesAsync(obj);
                UserRoles[obj.Id] = roles;
            }
        }
    }
}
