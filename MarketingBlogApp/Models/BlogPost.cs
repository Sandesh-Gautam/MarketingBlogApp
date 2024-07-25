using System.ComponentModel.DataAnnotations;

namespace MarketingBlogApp.Models
{
    public class BlogPost
    {
        public BlogPost()
        {
            Comments = new List<Comment>();
            Likes = new List<Like>();
            BlogPostCategories = new List<BlogPostCategory>();
        }
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime PublishedDate { get; set; }

        public string? ImageUrl { get; set; }

        public int ViewCount { get; set; }
        public bool IsVisible { get; set; } = true;

        public ApplicationUser Author { get; set; }
        public string AuthorId { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<BlogPostCategory> BlogPostCategories { get; set; }
    }
}
