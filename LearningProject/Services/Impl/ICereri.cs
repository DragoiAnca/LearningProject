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
           string? searchString = null);
    }
}
