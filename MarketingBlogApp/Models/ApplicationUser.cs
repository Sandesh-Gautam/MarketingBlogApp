using MarketingBlogApp.Models;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public string? ProfileImage { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDisabled { get; set; }
}
