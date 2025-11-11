namespace LearningProject.Models.ViewModels
{
    public class ViewModelPaginatedListCereri
    {

        public PaginatedList<Cereri> ListaCereriCuPaginatie { get; set; } = default!;
        public string? searchInput { get; set; }
        public string? sortOrder { get; set; }
        public int? pageNumber { get; set; }

        /*public int? nrCrt { get; set; }
        public string? name { get; set; }
        public string? descriere { get; set; }
        public string? creat_by { get; set; }
        public DateTime? data_creare { get; set; }
        public DateTime? data_stergere { get; set; }
        public string? sters_de { get; set; }
        public bool? status { get; set; }
        public int? pageNumber { get; set; } = 1;
        public string? sortOrder { get; set; }
        public string? searchString { get; set; } = "";

        // Lista paginată a ViewModel-urilor individuale
        public PaginatedList<Cereri>? ListaCereriCuPaginatie { get; set; }*/
    }
}
