using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MarketingBlogApp.Data;

namespace MarketingBlogApp.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> OnGet()
        {
            await LogUserActivity("Logged out");
            await _signInManager.SignOutAsync();
            return LocalRedirect("/");
        }

        public async Task<IActionResult> OnPost()
        {
            await LogUserActivity("Logged out");
            await _signInManager.SignOutAsync();
            return LocalRedirect("/");
        }

        private async Task LogUserActivity(string activity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    Activity = activity,
                    Timestamp = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
