using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Models.ViewModels;
using LearningProject.Services.Impl;
using Microsoft.EntityFrameworkCore;


//Metoda In-Memory
//Reîncărcarea datelor se face local, în procesul aplicației, prin metoda ReloadCacheAsync.
//Nu este Distributed Caching pentru că nu folosește un cache extern (Redis, SQL Server, etc.)
//și nu este partajat între instanțe ale aplicației.
namespace LearningProject.Services
{
    public class CereriCacheService : ICereriCacheService
    {
        private readonly LearningProjectContext _context;
        //asigură că doar un singur thread poate reîncărca cache-ul simultan.
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
        //datele actuale din cache
        public static ViewModelPaginatedListCereri? _cachedData; //Statice → există doar în memoria procesului curent al aplicației.
        //păstrează temporar datele vechi în timp ce se reîncarcă cache-ul
        public static ViewModelPaginatedListCereri? _cachedDataOLD;
        //urmărește când a fost ultima încărcare.
        public static DateTime _lastLoadTime = DateTime.MinValue;
        //permite crearea de scope-uri pentru dependențe, util pentru servicii Scoped
        private readonly IServiceScopeFactory _scopeFactory;
        //pentru log-uri și depanare
        private readonly ILogger<CereriCacheService> _logger;

        public CereriCacheService(
            IServiceScopeFactory scopeFactory,
            ILogger<CereriCacheService> logger, LearningProjectContext context)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _context = context;
        }


//1. Dacă cache-ul nu există sau a expirat(30 min), trebuie reîncărcat.
//2. Dacă e prima încărcare, așteaptă ReloadCacheAsync.
//3. Dacă nu e prima, returnează datele vechi (_cachedDataOLD) și în paralel reîncarcă cache-ul.
//4. Dacă cache-ul e încă valid, returnează _cachedData.

        public async Task<ViewModelPaginatedListCereri> GetCachedDataAsync(
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
            DateTime? data_stergere = null)
        {
            // Check if cache needs reload (first time or expired)
            if (_cachedData == null || (DateTime.UtcNow - _lastLoadTime).TotalMinutes >= 30)
            {
                if (_cachedData == null)
                {
                   await ReloadCacheAsync(sortOrder, pageNumber,
                        pageSize, searchString, filter, nrCrt, description, creat_de, sters_de, data_creare, data_stergere);

                }
                else 
                {
                    _cachedDataOLD = _cachedData;
                    ReloadCacheAsync(sortOrder, pageNumber,
                        pageSize, searchString, filter, nrCrt, description, creat_de, sters_de, data_creare, data_stergere);
                    return _cachedDataOLD;
                }
            }

            // Return cached data (with filters applied if needed)
            // Note: You might want to apply filters on the cached data here
            return _cachedData!;
        }

        //Obține date filtrate din DB
        public async Task<ViewModelPaginatedListCereri> GetFilteredCereriAsync(
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
            DateTime? data_stergere = null
)
        {
            var cereri = _context.Cereri
                .Include(c => c.CreatedByUser)
                .Include(c => c.DeletedBy)
                .Include(c => c.Documente)//o cerere are mai multe semnaturi
                .AsQueryable();

            // 🔹 Filtrare nume
            if (!string.IsNullOrEmpty(searchString))
            {
                cereri = cereri.Where(s => s.Name.Contains(searchString));
            }

            // 🔹 Filtrare nr cerere
            if (nrCrt.HasValue)
            {
                //din cauza key
                cereri = cereri.Where(s => s.value == nrCrt.Value);
            }

            if (!string.IsNullOrEmpty(description))
            {
                cereri = cereri.Where(s => s.Description.Contains(description));
            }

            if (!string.IsNullOrEmpty(creat_de))
            {
                cereri = cereri.Where(s => s.CreatedByUser != null &&
                                           s.CreatedByUser.Username.Contains(creat_de));
            }

            if (!string.IsNullOrEmpty(sters_de))
            {
                cereri = cereri.Where(s => s.DeletedBy != null &&
                                           s.DeletedBy.Username.Contains(sters_de));
            }

            // 🔹 Filtru dupa data creării 
            if (data_creare.HasValue)
            {
                cereri = cereri.Where(c =>
                    c.createdOn.Date == data_creare.Value.Date
                );
            }

            // 🔹 Filtru după data stergere 
            if (data_stergere.HasValue)
            {
                cereri = cereri.Where(c =>
        c.Deleted.HasValue &&
        c.Deleted.Value.Date == data_stergere.Value.Date);
            }

            // 🔹 Filtrare după dropdown
            switch (filter)
            {
                case "active":
                    cereri = cereri.Where(c => c.IsActive);
                    break;
                case "inactive":
                    cereri = cereri.Where(c => !c.IsActive);
                    break;
                case "toate":
                default:
                    break;
            }

            // Sortează DESC după Id
            cereri = cereri.OrderByDescending(c => c.Id);


            // 🔹 Sortare
            switch (sortOrder)
            {
                case "id":
                    cereri = cereri.OrderByDescending(c => c.Name);
                    break;
                case "name":
                    cereri = cereri.OrderByDescending(c => c.Name);
                    break;
                case "nrCrt":
                    cereri = cereri.OrderByDescending(c => c.value);
                    break;
                case "created_on_desc":
                    cereri = cereri.OrderByDescending(c => c.createdOn);
                    break;
                case "is_active_desc":
                    cereri = cereri.OrderByDescending(c => c.IsActive);
                    break;
                case "deletedBy":
                    cereri = cereri.OrderByDescending(c => c.DeletedBy);
                    break;
                case "deletedOn":
                    cereri = cereri.OrderBy(c => c.Deleted);
                    break;
                case "active":
                    cereri = cereri.OrderBy(c => c.IsActive);
                    break;
                case "status":
                    cereri = cereri.Where(c => c.IsActive);
                    break;
                case "description":
                    cereri = cereri.OrderBy(c => c.Description);
                    break;
                case "toate":
                    cereri = cereri.OrderBy(c => c.Name);
                    break;
                default:
                    cereri = cereri.OrderByDescending(c => c.Id);
                    break;
            }

            var paginatedCereri = await PaginatedList<Cereri>.CreateAsync(
                cereri.AsNoTracking(), pageNumber, pageSize);

            var listaCereriCuStatus = paginatedCereri.Select(c => new CerereStatusViewModel
            {
                Cerere = c,
                AllSigned = c.Documente.Any() && c.Documente.All(s => s.Status == StatusDocument.Semnat)
            }).ToList();


            var viewModel = new ViewModelPaginatedListCereri
            {
                ListaCereriCuPaginatie = paginatedCereri,
                sortOrder = sortOrder,
                searchInput = searchString,
                pageNumber = pageNumber,
                filter_value = filter,
                nrCrt = nrCrt,
                data_creare = data_creare,
                data_stergere = data_stergere,
                ListaCereriCuStatus = listaCereriCuStatus
            };

            return viewModel;
        }



        //scurta descriere: ReloadCacheAsync – Reîncărcarea cache-ului
        //Explicatie
//1. Se obține lock-ul pentru a preveni mai multe reîncărcări simultane.
//2. Se creează un scope pentru a rezolva dependențele scoped.
//3. Se reîncarcă datele filtrate din DB.
//4. Se actualizează _lastLoadTime.
//5. Dacă apare o eroare, se loghează și se propaga.
        public async Task ReloadCacheAsync(string sortOrder,
            int pageNumber = 1,
            int pageSize = 3,
            string? searchString = null,
            string? filter = "toate",
            double? nrCrt = null,
            string? description = null,
            string? creat_de = null,
            string? sters_de = null,
            DateTime? data_creare = null,
            DateTime? data_stergere = null)
        {
            await _cacheLock.WaitAsync();
            try
            {
                _logger.LogInformation("Reloading cache at {Time}", DateTime.UtcNow);

                // Create a scope to resolve scoped dependencies
                using var scope = _scopeFactory.CreateScope();
                var yourService = scope.ServiceProvider.GetRequiredService<ICereriCacheService>();//edit here

                // Load fresh data
                //edit here

                _cachedData = await GetFilteredCereriAsync(sortOrder, pageNumber,
                    pageSize, searchString, filter, nrCrt, description, creat_de, sters_de, data_creare, data_stergere);

                _lastLoadTime = DateTime.UtcNow;
                _logger.LogInformation("Cache reloaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading cache");
                throw;
            }
            finally
            {
                _cacheLock.Release();
            }
        }
    }
}
