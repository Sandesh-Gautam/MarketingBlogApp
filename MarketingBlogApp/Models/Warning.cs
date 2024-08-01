namespace MarketingBlogApp.Models
{
    public class Warning
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string Reason { get; set; }
        public DateTime DateIssued { get; set; }
        public bool IsResolved { get; set; }
        public int? BlogPostId { get; set; }
        public int? CommentId { get; set; }
    }
}
