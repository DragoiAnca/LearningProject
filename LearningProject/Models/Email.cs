using System.ComponentModel.DataAnnotations;

namespace LearningProject.Models
{
    public class Email
    {
        [Required]
        public string Title { get; set; } = "";
        [Required]
        public string Body { get; set; } = "";
        [Required]
        public string To { get; set; } = "";
        [Required]
        public string SendingProfile { get; set; } = "";
        public string? SenderID { get; set; }
        public string? Bcc { get; set; }
        public string? Cc { get; set; }
    }
}
