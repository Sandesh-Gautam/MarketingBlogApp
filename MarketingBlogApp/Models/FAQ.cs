using System.ComponentModel.DataAnnotations;

namespace MarketingBlogApp.Models
{
    public class FAQ
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The question is required.")]
        [StringLength(500, ErrorMessage = "The question cannot be longer than 500 characters.")]
        public string Question { get; set; }

        [Required(ErrorMessage = "The answer is required.")]
        [StringLength(2000, ErrorMessage = "The answer cannot be longer than 2000 characters.")]
        public string Answer { get; set; }
    }
}
