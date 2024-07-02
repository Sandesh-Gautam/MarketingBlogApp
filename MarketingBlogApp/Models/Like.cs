using System.ComponentModel.DataAnnotations;

namespace MarketingBlogApp.Models
{
    public class Like
    {
        public int Id { get; set; }

        public ApplicationUser User { get; set; }
        public string UserId { get; set; }

        public BlogPost BlogPost { get; set; }
        public int BlogPostId { get; set; }
    }
}
