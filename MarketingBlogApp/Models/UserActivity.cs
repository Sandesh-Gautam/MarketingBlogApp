using System;

namespace MarketingBlogApp.Models
{
    public class UserActivity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Activity { get; set; }
        public DateTime Timestamp { get; set; }

        // Navigation property to link to ApplicationUser
        public ApplicationUser User { get; set; }
    }
}
