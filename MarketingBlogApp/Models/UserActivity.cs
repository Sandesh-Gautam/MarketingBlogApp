using System;

namespace MarketingBlogApp.Models
{
    public class UserActivity
    {
        public int Id { get; set; }
        public ApplicationUser User { get; set; }
        public string UserId { get; set; }
        public string ActivityType { get; set; }
        public DateTime ActivityDate { get; set; }
    }
}
