using MarketingBlogApp.Models;
public class Like
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int BlogPostId { get; set; }
    public DateTime CreatedAt { get; set; }

    public ApplicationUser User { get; set; }
    public BlogPost BlogPost { get; set; }
}
