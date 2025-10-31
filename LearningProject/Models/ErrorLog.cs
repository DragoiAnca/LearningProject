using System.ComponentModel.DataAnnotations;

namespace LearningProject.Models
{
    public class ErrorLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string ErrorMessage { get; set; }

        [MaxLength(1000)]
        public string? StackTrace { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateOccurred { get; set; } = DateTime.Now;
    }
}
