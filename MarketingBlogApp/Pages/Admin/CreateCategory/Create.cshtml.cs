using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketingBlogApp.Data;
using MarketingBlogApp.Models;

namespace MarketingBlogApp.Pages.Admin.CreateCategory
{
    public class CreateModel : PageModel
    {
        private readonly MarketingBlogApp.Data.ApplicationDbContext _context;

        public CreateModel(MarketingBlogApp.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Category Category { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
  public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
    {
        // Debug or log ModelState errors to identify the issue
        foreach (var key in ModelState.Keys)
        {
            var state = ModelState[key];
            foreach (var error in state.Errors)
            {
                Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
            }
        }

        return Page();
    }

    try
    {
        _context.Categories.Add(Category);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
    catch (Exception ex)
    {
        // Log or handle exceptions during save operation
        Console.WriteLine($"Exception occurred: {ex.Message}");
        throw;
    }
}

    }
}
