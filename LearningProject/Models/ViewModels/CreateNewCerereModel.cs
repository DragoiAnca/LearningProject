using System.ComponentModel.DataAnnotations;

namespace LearningProject.Models.ViewModels
{
    public class CreateNewCerereModel
    {
        [Required(ErrorMessage = "Denumirea este obligatorie.")]
        [StringLength(100, ErrorMessage = "Denumirea nu poate depăși 100 de caractere.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Descrierea este obligatorie.")]
        [StringLength(250, ErrorMessage = "Descrierea nu poate depăși 250 de caractere.")]
        public string Description { get; set; }


        [Required(ErrorMessage = "Numărul este obligatoriu.")]
        [Range(1, 1000000, ErrorMessage = "Numărul trebuie să fie între 1 și 1.000.000.")]
        public double? Value { get; set; }
    }
}
