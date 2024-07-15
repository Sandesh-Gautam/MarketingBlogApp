using MarketingBlogApp.Data;
using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<BlogPost> BlogPosts { get; set; }
        public List<SelectListItem> CategoryOptions { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SearchCategory { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        public async Task OnGetAsync(string searchCategory, int? pageNumber)
        {
            const int pageSize = 5; // Set page size to 5
            CurrentPage = pageNumber ?? 1;
            CurrentPage = CurrentPage < 1 ? 1 : CurrentPage;

            // Ensure the guest user exists
            const string guestUserId = "1";
            var guestUser = await _userManager.FindByIdAsync(guestUserId);
            if (guestUser == null)
            {
                var newGuestUser = new ApplicationUser
                {
                    Id = guestUserId,
                    UserName = "guest",
                    Email = "guest@example.com",
                    EmailConfirmed = true,
                    Address = "Guest Address",
                    FirstName = "Guest",
                    LastName = "User"
                };
                var result = await _userManager.CreateAsync(newGuestUser);
                if (!result.Succeeded)
                {
                    throw new System.Exception("Failed to create guest user.");
                }
            }

            // Track the page visit
            var userId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : guestUserId;
            var activity = new UserActivity
            {
                UserId = userId,
                ActivityType = "Page Visit",
                ActivityDate = System.DateTime.Now
            };

            _context.UserActivities.Add(activity);
            await _context.SaveChangesAsync();

            // Fetch blog posts based on the search category
            var query = _context.BlogPosts
                .Include(bp => bp.BlogPostCategories)
                    .ThenInclude(bc => bc.Category)
                .Include(bp => bp.Comments)
                    .ThenInclude(c => c.User)
                .Include(bp => bp.Likes)
                .Include(bp => bp.Author)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchCategory) && SearchCategory != "All Categories")
            {
                query = query.Where(bp => bp.BlogPostCategories.Any(bc => bc.Category.Name == SearchCategory));
            }

            var totalCount = await query.CountAsync();
            TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);

            BlogPosts = await query
                .Skip((CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Fetch categories for the dropdown
            var categories = await _context.Categories.ToListAsync();
            CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.Name,
                Text = c.Name
            }).ToList();

            CategoryOptions.Insert(0, new SelectListItem { Value = "All Categories", Text = "All Categories" });
        }

        public async Task<IActionResult> OnPostAddCommentAsync(int postId, string commentContent)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (string.IsNullOrEmpty(commentContent))
            {
                ModelState.AddModelError("commentContent", "Comment content cannot be empty.");
                await OnGetAsync(SearchCategory, CurrentPage);
                return Page();
            }

            var post = await _context.BlogPosts
                .Include(bp => bp.Comments)
                .FirstOrDefaultAsync(bp => bp.Id == postId);

            if (post == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                Content = commentContent,
                CommentedDate = System.DateTime.Now,
                UserId = user.Id,
                BlogPostId = postId
            };

            post.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { searchCategory = SearchCategory, pageNumber = CurrentPage });
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId, string reason, IFormFile proofImage)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == comment.UserId)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return RedirectToPage(new { searchCategory = SearchCategory, pageNumber = CurrentPage });
            }

            // Save the proof image
            string proofImagePath = null;
            if (proofImage != null)
            {
                var filePath = Path.Combine("wwwroot/uploads/proof-images", proofImage.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await proofImage.CopyToAsync(stream);
                }
                proofImagePath = $"/uploads/proof-images/{proofImage.FileName}";
            }

            var manager = await _userManager.GetUserAsync(User);

            // Create a warning for the comment author
            var warning = new Warning
            {
                UserId = comment.UserId,
                Reason = reason,
                DateIssued = System.DateTime.Now,
                IsResolved = false
            };
            _context.Warnings.Add(warning);

            // Log the manager's action
            var managerAction = new ManagerAction
            {
                ManagerId = manager.Id,
                ActionType = "Deleted a comment",
                Reason = reason,
                ActionDate = System.DateTime.Now,
                ProofImagePath = proofImagePath
            };
            _context.ManagerActions.Add(managerAction);

            // Delete the comment
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { searchCategory = SearchCategory, pageNumber = CurrentPage });
        }

        public async Task<IActionResult> OnPostDeletePostAsync(int postId, string reason, IFormFile proofImage)
        {
            var post = await _context.BlogPosts.FindAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            // Save the proof image
            string proofImagePath = null;
            if (proofImage != null)
            {
                var filePath = Path.Combine("wwwroot/uploads/proof-images", proofImage.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await proofImage.CopyToAsync(stream);
                }
                proofImagePath = $"/uploads/proof-images/{proofImage.FileName}";
            }

            var manager = await _userManager.GetUserAsync(User);

            // Create a warning for the post author
            var warning = new Warning
            {
                UserId = post.AuthorId,
                Reason = reason,
                DateIssued = System.DateTime.Now,
                IsResolved = false
            };
            _context.Warnings.Add(warning);

            // Log the manager's action
            var managerAction = new ManagerAction
            {
                ManagerId = manager.Id,
                ActionType = "Deleted a post",
                Reason = reason,
                ActionDate = System.DateTime.Now,
                ProofImagePath = proofImagePath
            };
            _context.ManagerActions.Add(managerAction);

            // Delete the post
            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { searchCategory = SearchCategory, pageNumber = CurrentPage });
        }

        public async Task<IActionResult> OnPostToggleLikeAsync(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var post = await _context.BlogPosts
                .Include(bp => bp.Likes)
                .FirstOrDefaultAsync(bp => bp.Id == postId);

            if (post == null)
            {
                return NotFound();
            }

            var like = post.Likes.FirstOrDefault(l => l.UserId == user.Id);
            if (like == null)
            {
                like = new Like
                {
                    BlogPostId = postId,
                    UserId = user.Id
                };
                post.Likes.Add(like);
            }
            else
            {
                post.Likes.Remove(like);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { searchCategory = SearchCategory, pageNumber = CurrentPage });
        }
    }
}
