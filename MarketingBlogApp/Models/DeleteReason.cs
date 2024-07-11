using MarketingBlogApp.Models;

public class DeletionReason
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string Reason { get; set; }
    public DateTime DateIssued { get; set; }
    public bool IsResolved { get; set; }
    public string ProofImageUrl { get; set; }
}
