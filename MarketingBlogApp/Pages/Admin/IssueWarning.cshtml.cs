using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin
{
    public class IssueWarningModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IssueWarningModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Reason { get; set; }

        public ManagerAction ManagerAction { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ManagerAction = await _context.ManagerActions
                .Include(ma => ma.Manager)
                .FirstOrDefaultAsync(ma => ma.Id == id);

            if (ManagerAction == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var managerAction = await _context.ManagerActions.FindAsync(id);
            if (managerAction == null)
            {
                return NotFound();
            }

            var warning = new Warning
            {
                UserId = managerAction.ManagerId,
                Reason = Reason,
                DateIssued = DateTime.Now,
                IsResolved = false
            };

            _context.Warnings.Add(warning);

            // Check if user has 3 unresolved warnings
            var warningsCount = await _context.Warnings
                .CountAsync(w => w.UserId == managerAction.ManagerId && !w.IsResolved);

            if (warningsCount >= 3)
            {
                var user = await _context.Users.FindAsync(managerAction.ManagerId);
                user.IsDisabled = true;
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Warning issued successfully.";
            return RedirectToPage("./Dashboard");
        }
    }
}
