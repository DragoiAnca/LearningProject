using LearningProject.Models.ViewModels;

namespace LearningProject.Services.Impl
{
    public interface ICereriCacheService
    {
        Task<ViewModelPaginatedListCereri> GetCachedDataAsync(
        string sortOrder,
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

        Task ReloadCacheAsync();
    }
}
