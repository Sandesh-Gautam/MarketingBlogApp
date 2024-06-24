using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _context;

        public EditModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<EditModel> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public ApplicationUser UserToUpdate { get; set; }

        [BindProperty]
        public string SelectedRole { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }
        public bool IsAdmin { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            UserToUpdate = await _userManager.FindByIdAsync(id);
            if (UserToUpdate == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    Activity = "Viewed Edit User Page",
                    Timestamp = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }

            var userRoles = await _userManager.GetRolesAsync(UserToUpdate);
            IsAdmin = userRoles.Contains("Admin");
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

            var existingUserWithUsername = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == UserToUpdate.UserName && u.Id != id);
            if (existingUserWithUsername != null)
            {
                ModelState.AddModelError("UserToUpdate.UserName", "Username is already taken.");
                return Page();
            }

            var existingUserWithEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == UserToUpdate.Email && u.Id != id);
            if (existingUserWithEmail != null)
            {
                ModelState.AddModelError("UserToUpdate.Email", "Email is already taken.");
                return Page();
            }

            user.FirstName = UserToUpdate.FirstName;
            user.LastName = UserToUpdate.LastName;
            user.Address = UserToUpdate.Address;
            user.Email = UserToUpdate.Email;
            user.UserName = UserToUpdate.UserName;

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Contains("Admin"))
            {
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
            }
            else
            {
                SelectedRole = "Admin";
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (user != null)
            {
                var editUsers = new UserActivity
                {
                    UserId = user.Id,
                    Activity = "Edited a User",
                    Timestamp = DateTime.Now
                };
                _context.UserActivities.Add(editUsers);
                await _context.SaveChangesAsync();
            }
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to update user.");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
