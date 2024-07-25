using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MarketingBlogApp.Pages.Admin
{
    public class MessagesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MessagesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ContactMessage> Messages { get; set; }

        public async Task OnGetAsync()
        {
            Messages = await _context.ContactMessages.ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.ContactMessages.Remove(message);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Message deleted successfully.";
            return RedirectToPage();
        }
    }
}
