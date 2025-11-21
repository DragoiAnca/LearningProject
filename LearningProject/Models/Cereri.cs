using static LearningProject.Models.Signature;
using System.ComponentModel.DataAnnotations;
using LearningProject.Models.DraftModel;

namespace LearningProject.Models
{
    public partial class Cereri
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Denumirea cererii este obligatorie.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Descrierea cererii este obligatorie.")]
        public string Description { get; set; }
        public DateTime createdOn { get; set; }
        public bool IsActive { get; set; }
        public DateTime? Deleted { get; set; }
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }
        public int? DeletedById { get; set; }
        public User? DeletedBy { get; set; }
        [Required(ErrorMessage = "Valoarea este necesară.")]
        [Range(1, 100000, ErrorMessage = "Valoarea trebuie să fie între {1} și {2}.")]
        public double value { get; set; }

        // AICI E CHEIA
        public int? OldCereriId { get; set; }
        public Cereri? OldCereri { get; set; }
        public int? VersionNumber { get; set; }

        // Lista de fisiere asociate
        public ICollection<CerereFile> Files { get; set; } = new List<CerereFile>();
        
        //integrarea Draft
        public bool isDraft {  get; set; } //true sau false 

    }
}