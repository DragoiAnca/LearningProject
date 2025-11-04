using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace LearningProject.Models
{
    public class User
    {
        [Key]
        public int IdUser { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

        [EmailAddress]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Must be a valid Email Address")]
        public string Email { get; set; }

        //cheie straina
        public int roluriID { get; set; }

        [ValidateNever]
        public Roluri roluri { get; set; }

        public int id_departament { get; set; }
        public virtual Departamente? Departamente { get; set; }

        [DataType(DataType.Date)]
        public DateTime data_time { get; set; }

        [DataType(DataType.Date)]
        public DateTime? data_dezactivare { get; set; }

    }
}
