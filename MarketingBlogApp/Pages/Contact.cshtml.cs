using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MarketingBlogApp.Pages
{
    [Authorize]
    public class ContactModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ContactModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ContactMessage Contact { get; set; }

        public bool MessageSent { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.ContactMessages.Add(Contact);
            await _context.SaveChangesAsync();

            // Clear the fields
            ModelState.Clear();

            // Reset the Contact property
            Contact = new ContactMessage();

            MessageSent = true;
            return Page();
        }
    }
}
