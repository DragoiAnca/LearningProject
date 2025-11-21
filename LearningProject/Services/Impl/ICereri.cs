using LearningProject.Models;
using LearningProject.Models.ViewModels;

namespace LearningProject.Services.Impl
{
    public interface ICereri
    {
        Task<ViewModelPaginatedListCereri> GetFilteredCereriAsync(
           string sortOrder,
           int pageNumber = 1,
           int pageSize = 3,
           string? searchString = null,
           string filter = "toate",
           double?  nrCrt = null,
           string? description = null,
           string? creat_de = null,
           string? sters_de = null,
           DateTime? data_creare = null,
           DateTime? data_stergere = null
           );

        Task<Cereri> CreateCerereAsync(
            CreateNewCerereModel model,
            string currentUserName,
            List<IFormFile>? files = null);

        Task ReloadCacheAsync(string sortOrder,
        int pageNumber = 1,
        int pageSize = 3,
        string? searchString = null,
        string? filter = "toate",
        double? nrCrt = null,
        string? description = null,
        string? creat_de = null,
        string? sters_de = null,
        DateTime? data_creare = null,
        DateTime? data_stergere = null);
    }

}

