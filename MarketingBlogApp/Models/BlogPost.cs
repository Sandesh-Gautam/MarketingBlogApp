using System;
using System.Collections.Generic;

namespace MarketingBlogApp.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? Picture { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorId { get; set; }
    }
}
