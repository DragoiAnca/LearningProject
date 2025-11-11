using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Models.ViewModels;
using LearningProject.Services.Impl;
using Microsoft.EntityFrameworkCore;

namespace LearningProject.Services
{
    public class CereriService : ICereri
    {
        private readonly LearningProjectContext _context;

        public CereriService(LearningProjectContext context)
        {
            _context = context;
        }

        // 🔹 Modificăm tipul returnat: ViewModelPaginatedListCereri
        public async Task<ViewModelPaginatedListCereri> GetFilteredCereriAsync(
            string sortOrder,
            int pageNumber = 1,
            int pageSize = 3,
            string? searchString = null)
        {
            var cereri = _context.Cereri
                .Include(c => c.CreatedByUser)
                .Include(c => c.DeletedBy)
                .AsQueryable();

            // 🔹 Filtrare
            if (!string.IsNullOrEmpty(searchString))
            {
                cereri = cereri.Where(s => s.Name.Contains(searchString));
            }

            // 🔹 Sortare
            switch (sortOrder)
            {
                case "name_desc":
                    cereri = cereri.OrderByDescending(c => c.Name);
                    break;
                case "created_by_desc":
                    cereri = cereri.OrderByDescending(c => c.CreatedByUser);
                    break;
                case "created_on_desc":
                    cereri = cereri.OrderByDescending(c => c.createdOn);
                    break;
                case "is_active_desc":
                    cereri = cereri.OrderByDescending(c => c.IsActive);
                    break;
                case "delete_by_desc":
                    cereri = cereri.OrderByDescending(c => c.DeletedBy);
                    break;
                case "status_desc":
                    cereri = cereri.OrderBy(c => c.Deleted);
                    break;
                case "active":
                    cereri = cereri.Where(c => c.IsActive);
                    break;
                case "inactive":
                    cereri = cereri.Where(c => !c.IsActive);
                    break;
                case "toate":
                    cereri = cereri.OrderBy(c => c.Name);
                    break;
                default:
                    cereri = cereri.OrderBy(c => c.Name);
                    break;
            }

            // 🔹 Paginare
            var paginatedCereri = await PaginatedList<Cereri>.CreateAsync(
                cereri.AsNoTracking(), pageNumber, pageSize);

            // 🔹 Construim view modelul pentru view
            var viewModel = new ViewModelPaginatedListCereri
            {
                ListaCereriCuPaginatie = paginatedCereri,
                sortOrder = sortOrder,
                searchInput = searchString,
                pageNumber = pageNumber
            };

            return viewModel;
        }
    }
}
