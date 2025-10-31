using System.ComponentModel.DataAnnotations;

namespace LearningProject.Models
{
    public class Roluri
    {
        [Key]
        public int IdRol { get; set; }

        [Required]
        [MaxLength(250)]
        public string Denumire_rol { get; set; }

        public virtual ICollection<Claim>? Claims { get; set; }
        // One Role can have many Users
        public virtual ICollection<User>? Users { get; set; } = new List<User>();

    }
}
