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

        public IList<Warning> Warnings { get; set; }

        public WarningsModel(ILogger<WarningsModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Warnings = await _context.Warnings
                .Include(w => w.User)
                .ToListAsync();
        }
    }
}
