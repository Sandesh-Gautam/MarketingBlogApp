using System;

namespace MarketingBlogApp.Models
{
    public class TrendingBlog
    {
        public int Id { get; set; }
        public DateTime RecordedAt { get; set; }
        public int TrendingBlogPostId { get; set; }
        public BlogPost BlogPost { get; set; }
    }
}
