namespace LearningProject.Models.ViewModels
{
    public class UserEditViewModel
    {
        public int IdUser { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public int? roluriID;
        public int? id_departament;
    }
}
