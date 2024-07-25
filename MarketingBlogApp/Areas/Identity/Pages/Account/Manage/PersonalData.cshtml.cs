// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Threading.Tasks;
using MarketingBlogApp.Models;
using MarketingBlogApp.Data; // Add this using statement for ApplicationDbContext
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MarketingBlogApp.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PersonalDataModel> _logger;
        private readonly ApplicationDbContext _context; // Add this field for ApplicationDbContext

        public PersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<PersonalDataModel> logger,
            ApplicationDbContext context) // Update the constructor to include ApplicationDbContext
        {
            _userManager = userManager;
            _logger = logger;
            _context = context; // Assign the context
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Log the "Visited Personal Data Page" activity
            LogUserActivity("Visited Personal Data Page");

            return Page();
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
