using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningProject.Models
{
    [Keyless]  // Keyless = view read-only
    public class V_employees
    {
        [Column("EmployeeID")]
        public string? EmployeeID { get; set; }
        [Column("LastName")]
        public string? LastName { get; set; }
        [Column("FirstName")]
        public string? FirstName { get; set; }
        [Column("FullName")]
        public string? FullName { get; set; }
        [Column("StartDate")]
        public DateTime? StartDate { get; set; }
        [Column("EndDate")]
        public DateTime? EndDate { get; set; }
        [Column("Department")]
        public string? Department { get; set; }
        [Column("Title")]
        public string? Title { get; set; }
        [Column("Activity")]
        public string? Activity { get; set; }
        [Column("Username")]
        public string? Username { get; set; }
        [Column("Email")]
        public string? Email { get; set; }
        [Column("Manager")]
        public string? Manager { get; set; }
    }
}
