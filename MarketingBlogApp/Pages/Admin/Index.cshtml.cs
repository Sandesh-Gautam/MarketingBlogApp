using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IList<ApplicationUser> Users { get; set; }
        public Dictionary<string, IList<string>> UserRoles { get; set; }

        public async Task OnGetAsync()
        {
            var users = _userManager.Users.ToList();
            var userRole = await _roleManager.FindByNameAsync("User");

            Users = new List<ApplicationUser>();
            UserRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User") || roles.Contains("Manager"))
                {
                    Users.Add(user);
                    UserRoles[user.Id] = roles;
                }
            }
        }
    }
}
