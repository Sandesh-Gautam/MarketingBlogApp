using System;

namespace MarketingBlogApp.Models
{
    public class PopularBlog
    {
        public int Id { get; set; }
        public int BlogPostId { get; set; }
        public int ViewCount { get; set; }
    }

}
