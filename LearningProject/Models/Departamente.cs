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

        public bool isActive { get; set; }

        [DataType(DataType.Date)]
        public DateTime data_inceput { get; set; }

        [DataType(DataType.Date)]
        public DateTime? data_dezactivare { get; set; }


    }
}
