using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class DisabledUsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DisabledUsersModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<ApplicationUser> DisabledUsers { get; set; }

        public async Task OnGetAsync()
        {
            DisabledUsers = await _userManager.Users.Where(u => u.IsDisabled).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsDisabled = true; // Disable the user
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to disable user.");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
