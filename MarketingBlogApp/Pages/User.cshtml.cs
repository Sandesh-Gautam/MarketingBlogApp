using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarketingBlogApp.Pages
{
    [Authorize]
    public class UserModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ApplicationUser? appUser;
        public UserModel(UserManager<ApplicationUser> userManager)
        {
            this.UserManager = userManager;
           
        }

        public UserManager<ApplicationUser> UserManager { get; }

        public void OnGet()
        {
            var task = UserManager.GetUserAsync(User);
            task.Wait();
            appUser = task.Result;

        }
    }
}
