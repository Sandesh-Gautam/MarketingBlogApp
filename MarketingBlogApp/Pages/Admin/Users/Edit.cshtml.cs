using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<EditModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [BindProperty]
        public ApplicationUser UserToUpdate { get; set; }

        [BindProperty]
        public string SelectedRole { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            UserToUpdate = await _userManager.FindByIdAsync(id);
            if (UserToUpdate == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(UserToUpdate);
            SelectedRole = userRoles.FirstOrDefault();

            Roles = _roleManager.Roles.Where(r => r.Name != "Admin").Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            });

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                Roles = _roleManager.Roles.Where(r => r.Name != "Admin").Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                });

                return Page();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = UserToUpdate.FirstName;
            user.LastName = UserToUpdate.LastName;
            user.Address = UserToUpdate.Address;
            user.Email = UserToUpdate.Email;
            user.UserName = UserToUpdate.UserName;

            var currentRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to update user roles.");
                return Page();
            }

            result = await _userManager.AddToRoleAsync(user, SelectedRole);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to update user roles.");
                return Page();
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to update user.");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
