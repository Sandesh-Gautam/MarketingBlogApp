using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text;
using System.Text.Encodings.Web;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MarketingBlogApp.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class CreateUsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CreateUsersModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public CreateUsersModel(UserManager<ApplicationUser> userManager, ILogger<CreateUsersModel> logger, IEmailSender emailSender, ApplicationDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "The First Name Field is required.")]
            [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First Name cannot contain numbers or special characters.")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "The Last Name Field is required.")]
            [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last Name cannot contain numbers or special characters.")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "The Email Field is required.")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "The Username Field is required.")]
            public string UserName { get; set; }

            [Required(ErrorMessage = "The Address Field is required.")]
            public string Address { get; set; }
        }

        public void OnGet()
        {
            // Log the "Viewed Create Users Page" activity
            LogUserActivity("Viewed Create Users Page");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrEmpty(Input.Email))
            {
                ModelState.AddModelError(string.Empty, "Email address is required.");
                return Page();
            }

            var existingEmail = await _userManager.FindByEmailAsync(Input.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                return Page();
            }

            // Check if the username is already taken
            var existingUsername = await _userManager.FindByNameAsync(Input.UserName);
            if (existingUsername != null)
            {
                ModelState.AddModelError(string.Empty, "Username is already taken.");
                return Page();
            }

            // Generate a temporary password
            var temporaryPassword = GenerateTemporaryPassword();

            var user = new ApplicationUser
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                UserName = Input.UserName,
                Email = Input.Email,
                ProfileImage = "/uploads/BlankProfile.jpeg",
                Address = Input.Address,
                CreatedAt = DateTime.Now,
            };

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var hashedPassword = passwordHasher.HashPassword(user, temporaryPassword);

            // Set the hashed password
            user.PasswordHash = hashedPassword;

            // Create the user
            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                if (user != null)
                {
                    var createUsers = new UserActivity
                    {
                        UserId = user.Id,
                        ActivityType = "Created User",
                        ActivityDate = DateTime.Now
                    };
                    _context.UserActivities.Add(createUsers);
                    await _context.SaveChangesAsync();
                }
                // Assign the user to a role (example: Manager role)
                await _userManager.AddToRoleAsync(user, "Manager");

                _logger.LogInformation("User created a new account with temporary password.");

                try
                {
                    // Generate password reset token
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                    // Construct the callback URL for setting up password
                    var callbackUrl = Url.Page(
                        "/ResetPassword",
                        pageHandler: null,
                        values: new { code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)), email = user.Email },
                        protocol: Request.Scheme);

                    // Send an email to the user with the temporary password and password setup link
                    await _emailSender.SendEmailAsync(Input.Email, "Set up your password",
                        $"Your temporary password is {temporaryPassword}. Please set up your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    TempData["SuccessMessage"] = "User created successfully.";
                    return RedirectToPage("/Admin/Dashboard");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to user.");
                    ModelState.AddModelError(string.Empty, "Failed to send email. Please try again later.");
                    return Page();
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }


        private string GenerateTemporaryPassword(int length = 12)
        {
            if (length < 4)
                throw new ArgumentException("Password length must be at least 4.", nameof(length));

            const string lowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
            const string upperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digitChars = "1234567890";
            const string specialChars = "!@#$%^&*()";
            const string allChars = lowerCaseChars + upperCaseChars + digitChars + specialChars;

            StringBuilder res = new StringBuilder();
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                // Ensure at least one of each required character type
                res.Append(GetRandomChar(lowerCaseChars, rng, uintBuffer));
                res.Append(GetRandomChar(upperCaseChars, rng, uintBuffer));
                res.Append(GetRandomChar(digitChars, rng, uintBuffer));
                res.Append(GetRandomChar(specialChars, rng, uintBuffer));

                // Fill the rest of the password length with random characters from all character sets
                for (int i = 4; i < length; i++)
                {
                    res.Append(GetRandomChar(allChars, rng, uintBuffer));
                }
            }

            // Shuffle the resulting characters to avoid any predictable pattern
            return Shuffle(res.ToString());
        }

        private char GetRandomChar(string chars, RNGCryptoServiceProvider rng, byte[] uintBuffer)
        {
            rng.GetBytes(uintBuffer);
            uint num = BitConverter.ToUInt32(uintBuffer, 0);
            return chars[(int)(num % (uint)chars.Length)];
        }

        private string Shuffle(string input)
        {
            char[] array = input.ToCharArray();
            Random rng = new Random();
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
            return new string(array);
        }

        private void LogUserActivity(string activity)
        {
            var userActivity = new UserActivity
            {
                UserId = _userManager.GetUserId(User),
                ActivityType = activity,
                ActivityDate = DateTime.Now
            };
            _context.UserActivities.Add(userActivity);
            _context.SaveChanges();
        }
    }
}
