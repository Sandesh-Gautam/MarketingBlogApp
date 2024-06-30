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
    public class DetailsModel : PageModel
    {
        private readonly MarketingBlogApp.Data.ApplicationDbContext _context;

        public DetailsModel(MarketingBlogApp.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public BlogPost BlogPost { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogpost = await _context.BlogPosts.FirstOrDefaultAsync(m => m.Id == id);
            if (blogpost == null)
            {
                return NotFound();
            }
            else
            {
                BlogPost = blogpost;
            }
            return Page();
        }
    }
}
