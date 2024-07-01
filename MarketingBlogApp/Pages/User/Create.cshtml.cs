using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarketingBlogApp.Pages.User
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public BlogPost BlogPost { get; set; } = new BlogPost();

        [BindProperty]
        public IFormFile Picture { get; set; } // To bind the uploaded file

        [BindProperty]
        public List<int> CategoryIds { get; set; } = new List<int>(); // For handling selected category ids

        public IList<Category> Categories { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            BlogPost.AuthorId = user.Id; // Set AuthorId to logged-in user's ID

            Categories = await _context.Categories.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            BlogPost.CreatedAt = DateTime.UtcNow;
            BlogPost.AuthorId = user.Id; // Ensure this line is setting the AuthorId correctly

            // Handle file upload
            if (Picture != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Picture.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Picture.CopyToAsync(fileStream);
                }

                BlogPost.Picture = uniqueFileName;
            }

            // Validate Model manually
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Add BlogPost to DbContext
            _context.BlogPosts.Add(BlogPost);
            await _context.SaveChangesAsync();
            var topContributor = await _context.TopContributors
    .FirstOrDefaultAsync(tc => tc.AuthorId == user.Id);

            if (topContributor == null)
            {
                // If top contributor entry doesn't exist, create it
                topContributor = new TopContributor
                {
                    AuthorId = user.Id,
                    ContributionCount = 1 // Start with 1 if first post
                };
                _context.TopContributors.Add(topContributor);
            }
            else
            {
                // Increment contribution count
                topContributor.ContributionCount++;
            }

            // Create BlogPostCategory relationships
            foreach (var categoryId in CategoryIds)
            {
                var blogPostCategory = new BlogPostCategory
                {
                    BlogPostId = BlogPost.Id,
                    CategoryId = categoryId
                };
                _context.BlogPostCategories.Add(blogPostCategory);
            }

            // Save changes to add BlogPostCategory relationships
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
