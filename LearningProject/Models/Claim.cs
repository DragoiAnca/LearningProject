using System.ComponentModel.DataAnnotations;

namespace LearningProject.Models
{
    public class Claim
    {
       [Key]
       public int IdClaim { get; set; }
       public string name { get; set; }
       public ICollection<Roluri>? Roles { get; set; }
    }
}
