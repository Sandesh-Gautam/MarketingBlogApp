using System;

namespace MarketingBlogApp.Models
{
    public class PopularBlog
    {
        public int Id { get; set; }
        public DateTime RecordedAt { get; set; }
        public int PopularBlogPostId { get; set; }
        public BlogPost BlogPost { get; set; }
    }
}
