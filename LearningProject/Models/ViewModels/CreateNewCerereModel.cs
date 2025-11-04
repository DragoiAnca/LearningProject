using System.ComponentModel.DataAnnotations;

namespace LearningProject.Models.ViewModels
{
    public class CreateNewCerereModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [Required]

        public string Description { get; set; }
        [Required]
        public double? Value { get; set; }
    }
}
