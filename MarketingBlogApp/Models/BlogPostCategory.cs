﻿using MarketingBlogApp.Models;

namespace MarketingBlogApp.Models
{
    public class BlogPostCategory
    {
        public int BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
