using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MarketingBlogApp.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
                          UserManager<ApplicationUser> userManager,
                          ILogger<LoginModel> logger,
                          ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        [TempData]
        public string Warnings { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "The User Name field is required.")]
            public string Identifier { get; set; }

            [Required(ErrorMessage = "The Password field is required.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                ApplicationUser user = null;
                if (new EmailAddressAttribute().IsValid(Input.Identifier))
                {
                    user = await _userManager.FindByEmailAsync(Input.Identifier);
                }
                else
                {
                    user = await _userManager.FindByNameAsync(Input.Identifier);
                }

                if (user != null)
                {
                    var blacklisted = await _context.Blacklists.FirstOrDefaultAsync(b => b.Email == user.Email && b.IsActive);
                    if (blacklisted != null)
                    {
                        ModelState.AddModelError(string.Empty, "Your account has been blacklisted. Please contact support.");
                        return Page();
                    }

                    var unresolvedWarnings = await _context.Warnings
                        .Where(w => w.UserId == user.Id && !w.IsResolved)
                        .ToListAsync();

                    var warningsCount = unresolvedWarnings.Count;

                    if (warningsCount >= 3)
                    {
                        var lastWarning = unresolvedWarnings.OrderByDescending(w => w.DateIssued).First();
                        var disableUntil = lastWarning.DateIssued.AddDays(7);

                        if (DateTime.Now < disableUntil)
                        {
                            user.IsDisabled = true;
                            await _userManager.UpdateAsync(user);
                        }
                        else
                        {
                            user.IsDisabled = false;
                            await _userManager.UpdateAsync(user);
                        }
                    }

                    if (user.IsDisabled)
                    {
                        ModelState.AddModelError(string.Empty, "Your account is disabled. Please contact support.");
                        return Page();
                    }

                    var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        var loginActivity = new UserActivity
                        {
                            UserId = user.Id,
                            ActivityType = "Logged in",
                            ActivityDate = DateTime.Now
                        };
                        _context.UserActivities.Add(loginActivity);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("User logged in.");
                        TempData["message"] = "Logged In Successfully";

                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            return LocalRedirect(Url.Content("~/Admin/Dashboard"));
                        }
                        else if (await _userManager.IsInRoleAsync(user, "User") || await _userManager.IsInRoleAsync(user, "Manager"))
                        {
                            TempData["Warnings"] = string.Join("\n", unresolvedWarnings.Select(w => $"Warning: {w.Reason}"));

                            return LocalRedirect(Url.Content("~/Index"));
                        }

                        return LocalRedirect(returnUrl);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }
            return Page();
        }

    }
}
