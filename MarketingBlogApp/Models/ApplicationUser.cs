using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;

namespace MarketingBlogApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string? ProfileImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDisabled { get; set; }

        public ICollection<BlogPost> BlogPosts { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<UserActivity> UserActivities { get; set; }
    }
}

