using MarketingBlogApp.Data;
using Microsoft.AspNetCore.Identity;

namespace MarketingBlogApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Address { get; set; } = "";
        public string ProfileImage { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        internal ApplicationUser FirstOrDefault(Func<object, bool> value)
        {
            throw new NotImplementedException();
        }
    }
}
