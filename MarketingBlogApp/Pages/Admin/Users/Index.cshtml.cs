using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

        public IndexModel(UserManager<ApplicationUser> userManager,ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IList<ApplicationUser> Users { get; set; }
        public Dictionary<string, IList<string>> UserRoles { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    Activity = "Viewed Users",
                    Timestamp = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }
            var usersQuery = _userManager.Users;

            var users = await usersQuery.ToListAsync();
            Users = new List<ApplicationUser>();
            UserRoles = new Dictionary<string, IList<string>>();

            foreach (var obj in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Check if the user has roles other than "Admin"
                if (roles.Any(r => r != "Admin"))
                {
                    Users.Add(user);
                    UserRoles[user.Id] = roles;
                }
            }
        }
    }
}
