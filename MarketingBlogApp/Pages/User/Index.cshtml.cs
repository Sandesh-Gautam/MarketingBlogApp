using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;

namespace MarketingBlogApp.Pages.User
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<BlogPostViewModel> BlogPosts { get; set; }

        public async Task OnGetAsync()
        {
            var blogPosts = await _context.BlogPosts.ToListAsync(); // Retrieve all blog posts

            BlogPosts = blogPosts.Select(bp => new BlogPostViewModel
            {
                Id = bp.Id,
                Title = bp.Title,
                Content = bp.Content,
                Picture = bp.Picture,
                LikeCount = bp.LikeCount,
                CreatedAt = bp.CreatedAt,
                AuthorUserName = _context.Users.FirstOrDefault(u => u.Id == bp.AuthorId)?.UserName ?? "Unknown", // Use null coalescing operator
                Categories = _context.Categories
                    .Where(c => _context.BlogPostCategories
                        .Where(bpc => bpc.BlogPostId == bp.Id)
                        .Select(bpc => bpc.CategoryId)
                        .Contains(c.Id))
                    .ToList()
            }).ToList();
        }


        public class BlogPostViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string Picture { get; set; }
            public int LikeCount { get; set; }
            public DateTime CreatedAt { get; set; }
            public string AuthorUserName { get; set; }
            public List<Category> Categories { get; set; } = new List<Category>();
        }
    }
}
