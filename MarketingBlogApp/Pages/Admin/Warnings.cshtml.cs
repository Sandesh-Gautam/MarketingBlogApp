using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MarketingBlogApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class WarningsModel : PageModel
    {
        private readonly ILogger<WarningsModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IList<WarningViewModel> Warnings { get; set; }

        public WarningsModel(ILogger<WarningsModel> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            Warnings = await (from w in _context.Warnings
                              join dr in _context.DeletionReasons on w.UserId equals dr.UserId into wdr
                              from dr in wdr.DefaultIfEmpty()
                              select new WarningViewModel
                              {
                                  Id = w.Id,
                                  UserId = w.UserId,
                                  UserName = w.User.UserName,
                                  Reason = w.Reason,
                                  DateIssued = w.DateIssued,
                                  IsResolved = w.IsResolved,
                                  ProofImageUrl = dr.ProofImageUrl
                              }).ToListAsync();

            await LogUserActivity();
        }

        private async Task LogUserActivity()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    ActivityType = "Viewed Warnings",
                    ActivityDate = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class WarningViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
        public DateTime DateIssued { get; set; }
        public bool IsResolved { get; set; }
        public string ProofImageUrl { get; set; }
    }
}
