﻿using System;

namespace MarketingBlogApp.Models
{
    public class UserActivity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Activity { get; set; }
        public DateTime Timestamp { get; set; }
        public ApplicationUser User { get; set; }
    }
}
