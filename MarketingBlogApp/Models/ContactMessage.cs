using System;

namespace MarketingBlogApp.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime DateSubmitted { get; set; }
    }
}
