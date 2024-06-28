using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;

namespace MarketingBlogApp.Pages.Manager
{
    public class ViewPostsModel : PageModel
    {
        private readonly MarketingBlogApp.Data.ApplicationDbContext _context;

        public ViewPostsModel(MarketingBlogApp.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<BlogPostCategory> BlogPostCategory { get;set; } = default!;

        public async Task OnGetAsync()
        {
            BlogPostCategory = await _context.BlogPostCategories
                .Include(b => b.BlogPost)
                .Include(b => b.Category).ToListAsync();
        }
    }
}
