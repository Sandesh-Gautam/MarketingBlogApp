using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarketingBlogApp.Data;

namespace MarketingBlogApp.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ResetPasswordModel> _logger;
        private readonly ApplicationDbContext _context;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager, ILogger<ResetPasswordModel> logger,ApplicationDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context; 
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Temporary Password")]
            public string TemporaryPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm New Password")]
            [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string Code { get; set; } // This will hold the token generated for password reset

            public string Email { get; set; }
        }

        public IActionResult OnGet(string code = null, string email = null)
        {
            if (code == null || email == null)
            {
                return BadRequest("A code and email must be supplied for password reset.");
            }
            else
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
                    Email = email
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Find user by email
            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("/Index");
            }

            // Verify the temporary password first
            var isTempPasswordValid = await _userManager.CheckPasswordAsync(user, Input.TemporaryPassword);
            if (!isTempPasswordValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid temporary password.");
                return Page();
            }

            // Reset user's password to the new password
            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);

            if (resetPasswordResult.Succeeded)
            {
                if (user != null)
                {
                    var resetActivity = new UserActivity
                    {
                        UserId = user.Id,
                        Activity = "Changed Password",
                        Timestamp = DateTime.Now
                    };
                    _context.UserActivities.Add(resetActivity);
                    await _context.SaveChangesAsync();
                }
                _logger.LogInformation("User password reset successfully.");

                // Optionally update the user entity to reflect password change
                await _userManager.UpdateAsync(user);

                return RedirectToPage("/Index");
            }

            foreach (var error in resetPasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
