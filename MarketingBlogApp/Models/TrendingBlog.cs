using System;

namespace MarketingBlogApp.Models
{
    public class TrendingBlog
    {
        public int Id { get; set; }
        public int BlogPostId { get; set; }
        public DateTime TrendingDate { get; set; }
    }

}
