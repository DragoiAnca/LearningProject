using LearningProject.Models;
using System.ComponentModel.DataAnnotations;


namespace LearningProject.Models
{
    public class Cereri
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime createdOn { get; set; }
        public bool IsActive { get; set; }
        public DateTime? Deleted { get; set; }

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        public int? DeletedById { get; set; }
        public User? DeletedBy { get; set; }
        public double value { get; set; }
    }
}