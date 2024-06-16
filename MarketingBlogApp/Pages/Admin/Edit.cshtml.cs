using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<EditModel> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EditModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<EditModel> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public ApplicationUser User { get; set; }

        [BindProperty]
        public string SelectedRole { get; set; }

    
        public IEnumerable<SelectListItem> Roles { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            User = await _userManager.FindByIdAsync(id);
            if (User == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(User);
            SelectedRole = userRoles.FirstOrDefault();

            Roles = _roleManager.Roles.Select(r => new SelectListItem
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
                Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                });

                // Log ModelState errors
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        _logger.LogError($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                    }
                }

                return Page();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = User.FirstName;
            user.LastName = User.LastName;
            user.Address = User.Address;
            user.Email = User.Email;
            user.UserName = User.UserName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to update user.");
                return Page();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeRolesResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to remove user roles.");
                    return Page();
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, SelectedRole);
            if (!addRoleResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add user role.");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
