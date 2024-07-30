using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace MarketingBlogApp.Pages.User
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public BlogPost BlogPost { get; set; }

        [BindProperty]
        public IFormFile? Image { get; set; }

        [BindProperty]
        public List<int> SelectedCategories { get; set; }

        public List<SelectListItem> CategoryOptions { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            BlogPost = await _context.BlogPosts
                .Include(bp => bp.BlogPostCategories)
                .ThenInclude(bc => bc.Category)
                .Include(bp => bp.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (BlogPost == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();
            CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            SelectedCategories = BlogPost.BlogPostCategories.Select(bc => bc.CategoryId).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var blogPostToUpdate = await _context.BlogPosts
                .Include(bp => bp.BlogPostCategories)
                .FirstOrDefaultAsync(bp => bp.Id == BlogPost.Id);

            if (blogPostToUpdate == null)
            {
                return NotFound();
            }

            // Remove validation errors for navigation properties and fields
            ModelState.Remove("BlogPost.Author");
            ModelState.Remove("BlogPost.AuthorId");
            ModelState.Remove("BlogPost.Comments");
            ModelState.Remove("BlogPost.Likes");
            ModelState.Remove("BlogPost.BlogPostCategories");

            // Retain the existing image URL if no new image is uploaded
            if (Image != null)
            {
                var fileName = Path.GetFileName(Image.FileName);
                var filePath = Path.Combine("wwwroot/uploads", fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                blogPostToUpdate.ImageUrl = "/uploads/" + fileName;
            }
            else
            {
                blogPostToUpdate.ImageUrl = BlogPost.ImageUrl;
            }

            blogPostToUpdate.Title = BlogPost.Title;
            blogPostToUpdate.Content = BlogPost.Content;

            // Update categories
            blogPostToUpdate.BlogPostCategories.Clear();
            foreach (var categoryId in SelectedCategories)
            {
                blogPostToUpdate.BlogPostCategories.Add(new BlogPostCategory
                {
                    BlogPostId = blogPostToUpdate.Id,
                    CategoryId = categoryId
                });
            }

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

            _context.Entry(blogPostToUpdate).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Log the activity for editing a blog post
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userActivity = new UserActivity
                {
                    UserId = user.Id,
                    ActivityType = "Edited a blog post",
                    ActivityDate = DateTime.Now
                };
                _context.UserActivities.Add(userActivity);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
