using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MarketingBlogApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<BlogPostCategory> BlogPostCategories { get; set; }
    }
}
