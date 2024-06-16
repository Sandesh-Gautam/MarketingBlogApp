using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarketingBlogApp.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminUserCreatedConfirmationModel : PageModel
    {

        public void OnGet()
        {
        }
    }
}
