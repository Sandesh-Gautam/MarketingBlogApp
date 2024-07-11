using System;

namespace MarketingBlogApp.Models
{
    public class ManagerAction
    {
        public int Id { get; set; }
        public string ManagerId { get; set; }
        public ApplicationUser Manager { get; set; }
        public string ActionType { get; set; }
        public DateTime ActionDate { get; set; }
        public int TargetId { get; set; }
        public string Reason { get; set; }
        public string ProofImagePath { get; set; }
    }
}
