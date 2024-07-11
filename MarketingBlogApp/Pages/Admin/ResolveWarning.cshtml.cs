using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin
{
    public class ResolveWarningModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ResolveWarningModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Warning Warning { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Warning = await _context.Warnings
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (Warning == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var warning = await _context.Warnings.FindAsync(id);

            if (warning == null)
            {
                return NotFound();
            }

            warning.IsResolved = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Warning resolved successfully.";
            return RedirectToPage("./Dashboard");
        }
    }
}
