using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public BlogPost BlogPost { get; set; }

        [BindProperty]
        public IFormFile? Image { get; set; } // Nullable IFormFile

        [BindProperty]
        public List<int> SelectedCategories { get; set; }

        public List<SelectListItem> CategoryOptions { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            BlogPost.AuthorId = user.Id;
            BlogPost.PublishedDate = DateTime.Now;

            if (Image != null)
            {
                var fileName = Path.GetFileName(Image.FileName);
                var filePath = Path.Combine("wwwroot/uploads", fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                BlogPost.ImageUrl = "/uploads/" + fileName;
            }
            else
            {
                BlogPost.ImageUrl = "/uploads/BlankPost.jpg";
            }

            // Remove the validation errors for navigation properties and explicit properties
            ModelState.Remove("BlogPost.Author");
            ModelState.Remove("BlogPost.AuthorId");
            ModelState.Remove("BlogPost.Comments");
            ModelState.Remove("BlogPost.Likes");
            ModelState.Remove("BlogPost.BlogPostCategories");
            ModelState.Remove("Image"); // Ensure Image is not validated as required

            if (!ModelState.IsValid)
            {
                var categories = await _context.Categories.ToListAsync();
                CategoryOptions = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
                return Page();
            }

            _context.BlogPosts.Add(BlogPost);
            await _context.SaveChangesAsync();

            foreach (var categoryId in SelectedCategories)
            {
                _context.BlogPostCategories.Add(new BlogPostCategory
                {
                    BlogPostId = BlogPost.Id,
                    CategoryId = categoryId
                });
            }
            await _context.SaveChangesAsync();

            // Log the activity for creating a blog post
            var userActivity = new UserActivity
            {
                UserId = user.Id,
                ActivityType = "Created a blog post",
                ActivityDate = DateTime.Now
            };
            _context.UserActivities.Add(userActivity);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
