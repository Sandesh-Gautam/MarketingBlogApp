using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class EnableModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public EnableModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public ApplicationUser User { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            User = await _userManager.FindByIdAsync(id);
            if (User == null || !User.IsDisabled)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.IsDisabled)
            {
                return NotFound();
            }

            user.IsDisabled = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to enable user.");
                return Page();
            }

            return RedirectToPage("./DisabledUsers");
        }
    }
}
