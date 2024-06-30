using System;
using MarketingBlogApp.Models; 

namespace MarketingBlogApp.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int BlogPostId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
