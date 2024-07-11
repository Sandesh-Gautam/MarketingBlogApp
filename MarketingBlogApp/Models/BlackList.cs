using System;

namespace MarketingBlogApp.Models
{
    public class BlackList
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public DateTime DateBlacklisted { get; set; }
        public bool IsActive { get; set; }
    }
}
