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

        public IList<BlogPost> BlogPost { get;set; } = default!;

        public async Task OnGetAsync()
        {
            BlogPost = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Category).ToListAsync();
        }
    }
}
