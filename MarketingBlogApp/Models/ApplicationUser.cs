using MarketingBlogApp.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MarketingBlogApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "The First Name Field is required.")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "The Last Name Field is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        public string LastName { get; set; } = "";
        public string Address { get; set; } = "";
        public string ProfileImage { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsDisabled { get; set; } = false;

        internal ApplicationUser FirstOrDefault(Func<object, bool> value)
        {
            throw new NotImplementedException();
        }
    }
}
