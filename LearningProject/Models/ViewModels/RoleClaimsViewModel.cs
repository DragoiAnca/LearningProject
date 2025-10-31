namespace LearningProject.Models.ViewModels
{
    public class RoleClaimsViewModel
    {
        public int RolId { get; set; }
        public string RolDenumire { get; set; }
        public List<ClaimCheckbox> Claims { get; set; } = new List<ClaimCheckbox>();
    }
}
