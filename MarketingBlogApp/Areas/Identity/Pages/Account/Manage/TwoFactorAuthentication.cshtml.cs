// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MarketingBlogApp.Areas.Identity.Pages.Account.Manage
{
    public class TwoFactorAuthenticationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<TwoFactorAuthenticationModel> _logger;
        private readonly ApplicationDbContext _context; // Add this line

        public TwoFactorAuthenticationModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TwoFactorAuthenticationModel> logger,
            ApplicationDbContext context) // Add this parameter
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context; // Initialize the context
        }

        public bool HasAuthenticator { get; set; }
        public int RecoveryCodesLeft { get; set; }
        [BindProperty]
        public bool Is2faEnabled { get; set; }
        public bool IsMachineRemembered { get; set; }
        [TempData]
        public string StatusMessage { get; set; }

        private void LogUserActivity(string activity)
        {
            var userActivity = new UserActivity
            {
                UserId = _userManager.GetUserId(User), // Get the current user's ID
                ActivityType = activity, // Activity type (e.g., "Viewed Two Factor Authentication")
                ActivityDate = DateTime.Now // The date and time of the activity
            };
            _context.UserActivities.Add(userActivity); // Add the new activity to the context
            _context.SaveChanges(); // Save changes to the database
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

            // Log the activity
            LogUserActivity("Viewed Two Factor Authentication");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.ForgetTwoFactorClientAsync();
            StatusMessage = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
            return RedirectToPage();
        }
    }
}
