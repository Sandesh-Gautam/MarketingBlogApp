using System;
using System.Collections.Generic;

namespace MarketingBlogApp.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Picture { get; set; }

        public string AuthorId { get; set; }
        public int? CategoryId { get; set; }
        public ApplicationUser Author { get; set; }
        public Category Category { get; set; }

        public ICollection<Like> Likes { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<BlogPostCategory> BlogPostCategories { get; set; }
        public ICollection<PopularBlog> PopularBlogs { get; set; }
        public ICollection<TrendingBlog> TrendingBlogs { get; set; }
    }
}
