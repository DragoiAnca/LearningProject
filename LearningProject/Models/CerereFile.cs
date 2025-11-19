using System.ComponentModel.DataAnnotations;


namespace LearningProject.Models
{
    public class CerereFile
    {
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; } 

        [Required]
        public string FilePath { get; set; }  

        public int CereriId { get; set; }
        public Cereri Cerere { get; set; }
    }
}
