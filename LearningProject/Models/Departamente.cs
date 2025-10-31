using System.ComponentModel.DataAnnotations;

namespace LearningProject.Models
{
    public class Departamente
    {
        [Key]
        public int id_departamente { get; set; }

        [Required]
        [MaxLength(250)]
        public string Denumire_departament { get; set; }

      //  public string IdUser { get; set; }
       // public User UserCheieStraina { get; set; }

    }
}
