using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using static LearningProject.Models.Signature;

namespace LearningProject.Models
{
    public class User
    {
        [Key]
        public int IdUser { get; set; }
        public string? Name { get; set; }//Fullname
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? EmployeeID { get; set; }

        [DataType(DataType.Date)]
        public DateTime data_time { get; set; }

        [DataType(DataType.Date)]
        public DateTime? data_dezactivare { get; set; }

        //cheie straina
        public int roluriID { get; set; }

        [ValidateNever]
        public Roluri roluri { get; set; }
        public int? id_departament { get; set; }
        public virtual Departamente? Departamente { get; set; }
        public ICollection<Signature> DocumenteSemnate { get; set; } = new List<Signature>();//Colectie semnaturi, nu documente
    }
}
