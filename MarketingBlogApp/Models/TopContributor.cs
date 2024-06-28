using System;

namespace MarketingBlogApp.Models
{
    public class TopContributor
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }
}
