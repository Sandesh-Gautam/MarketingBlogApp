using System;
using System.ComponentModel.DataAnnotations;

namespace MarketingBlogApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CommentedDate { get; set; }

        public ApplicationUser User { get; set; }
        public string UserId { get; set; }

        public BlogPost BlogPost { get; set; }
        public int BlogPostId { get; set; }
    }
}
