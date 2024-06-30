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
        private readonly MarketingBlogApp.Data.ApplicationDbContext _context;

        public IndexModel(MarketingBlogApp.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<BlogPostViewModel> BlogPosts { get; set; } = default!;

        public async Task OnGetAsync()
        {
            BlogPosts = await _context.BlogPosts
                .Join(
                    _context.Users,
                    blogPost => blogPost.AuthorId,
                    user => user.Id,
                    (blogPost, user) => new BlogPostViewModel
                    {
                        Id = blogPost.Id,
                        Title = blogPost.Title,
                        Content = blogPost.Content,
                        Picture = blogPost.Picture,
                        LikeCount = blogPost.LikeCount,
                        CreatedAt = blogPost.CreatedAt,
                        AuthorUserName = user.UserName
                    })
                .ToListAsync();
        }

        public class BlogPostViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string? Picture { get; set; }
            public int LikeCount { get; set; }
            public DateTime CreatedAt { get; set; }
            public string AuthorUserName { get; set; }
        }
    }
}
