using LearningProject.Models.ViewModels;

namespace LearningProject.Models.ViewModels
{
    public class ViewModelPaginatedListCereri
    {
        public PaginatedList<Cereri> ListaCereriCuPaginatie { get; set; } = default!;
        public double? nrCrt { get; set; }
        public string? searchInput { get; set; }
        public string? sortOrder { get; set; }
        public int? pageNumber { get; set; }
        public string? filter_value { get; set; }
        public string? descriere_value { get; set; }
        public string? creat_de { get; set; }
        public string? sters_de { get; set; }
        public DateTime? data_creare { get; set; }
        public DateTime? data_stergere { get; set; }
        public List<CerereStatusViewModel>? ListaCereriCuStatus { get; set; }
    }
}
