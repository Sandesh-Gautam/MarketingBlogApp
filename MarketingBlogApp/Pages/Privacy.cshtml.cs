using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages
{
    [Authorize]
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;  
        private readonly ApplicationDbContext _context;  

        public PrivacyModel(ILogger<PrivacyModel> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    Activity = "Viewed Privacy Page",
                    Timestamp = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
