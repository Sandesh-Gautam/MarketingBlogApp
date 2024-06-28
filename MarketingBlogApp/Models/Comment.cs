using System;
using MarketingBlogApp.Models; 

namespace MarketingBlogApp.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorId { get; set; }
        public int BlogPostId { get; set; }

        public ApplicationUser Author { get; set; }
        public BlogPost BlogPost { get; set; }
    }
}
