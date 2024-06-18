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

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<ApplicationUser> Users { get; set; }
        public Dictionary<string, IList<string>> UserRoles { get; set; }

        public async Task OnGetAsync()
        {
            var usersQuery = _userManager.Users;

            var users = await usersQuery.ToListAsync();
            Users = new List<ApplicationUser>();
            UserRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
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
