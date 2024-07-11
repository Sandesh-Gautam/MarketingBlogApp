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
    public class ManagerActionsModel : PageModel
    {
        private readonly ILogger<ManagerActionsModel> _logger;
        private readonly ApplicationDbContext _context;

        public IList<ManagerAction> ManagerActions { get; set; }

        public ManagerActionsModel(ILogger<ManagerActionsModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            ManagerActions = await _context.ManagerActions
                .Include(ma => ma.Manager)
                .ToListAsync();
        }
    }
}
