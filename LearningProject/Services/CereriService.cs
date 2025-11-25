using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Models.DraftModel;
using LearningProject.Models.ViewModels;
using LearningProject.Services.Impl;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LearningProject.Services
{
    public class CereriService : ICereri
    {
        private readonly LearningProjectContext _context;
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
        private readonly IBufferedFileUploadService _bufferedFileUploadService;
        private readonly string _fileStoragePath;


        private static ViewModelPaginatedListCereri? _cachedData;
        private static ViewModelPaginatedListCereri? _cachedDataOld;
        private static DateTime _lastLoad = DateTime.MinValue;


        public static ViewModelPaginatedListCereri loadedData;
        public CereriService(
            LearningProjectContext context,
            IBufferedFileUploadService bufferedFileUploadService,
             IConfiguration configuration)
        {
            _context = context;
            _bufferedFileUploadService = bufferedFileUploadService;
            _fileStoragePath = configuration["ApiUrls:FileStoragePath"];
        }

        public async Task<ViewModelPaginatedListCereri> GetCereriAsync(
       string sortOrder,
       int pageNumber = 1,
       int pageSize = 3,
       string? searchString = null,
       string filter = "toate",
       double? nrCrt = null,
       string? description = null,
       string? creat_de = null,
       string? sters_de = null,
       DateTime? data_creare = null,
       DateTime? data_stergere = null
   )
        {
            // Cache expirat sau prima încărcare?
            if (_cachedData == null || (DateTime.UtcNow - _lastLoad).TotalMinutes >= 30)
            {
                if (_cachedData == null)
                {
                    await ReloadCacheAsync(sortOrder, pageNumber, pageSize,
                        searchString, filter, nrCrt, description, creat_de, sters_de,
                        data_creare, data_stergere);

                    return _cachedData!;
                }
                else
                {
                    _cachedDataOld = _cachedData;

                    _ = ReloadCacheAsync(sortOrder, pageNumber, pageSize,
                        searchString, filter, nrCrt, description, creat_de, sters_de,
                        data_creare, data_stergere);

                    return _cachedDataOld!;
                }
            }

            return _cachedData!;
        }





        // 🔹 Modificăm tipul returnat: ViewModelPaginatedListCereri
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
                .Where(c => !c.isDraft)
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

        public async Task<Cereri> CreateCerereAsync(CreateNewCerereModel model, string currentUserName, List<IFormFile>? files)
        {
            var user = await _context.User.FirstOrDefaultAsync(w => w.Username == currentUserName);
            if (user == null) throw new Exception("User not found.");

            // Construim lista de semnături
            var signatureRoles = new Dictionary<string, int>
    {
        { "Manager", 2 },
        { "Admin", 3 },
        { "Checker", 1 }
    };

            ICollection<Signature> listaSignaturi = new List<Signature>();

            foreach (var item in signatureRoles)
            {
                var claim = await _context.Claim.FirstOrDefaultAsync(c => c.name == item.Key);
                if (claim != null)
                {
                    listaSignaturi.Add(new Signature
                    {
                        ClaimCanSign = claim,
                        order = item.Value
                    });
                }
            }

            // Verificăm dacă există draft cu același DraftId
            var draft = await _context.Cereri
                .Include(c => c.Files)
                .FirstOrDefaultAsync(c => c.Id == model.DraftId && c.isDraft);

            Cereri cerere;

            if (draft != null)
            {
                // Actualizăm draftul existent și îl transformăm în cerere finală
                draft.Name = model.Name;
                draft.Description = model.Description;
                draft.value = model.Value ?? draft.value;
                draft.IsActive = true;
                draft.isDraft = false; // nu mai e draft
                draft.VersionNumber = draft.VersionNumber ?? 1;
                draft.Documente = listaSignaturi;

                cerere = draft;

                // FORȚĂM EF să marcheze entitatea ca modificată
                _context.Entry(cerere).State = EntityState.Modified;
            }
            else
            {
                // Nu există draft → creăm cerere nouă
                cerere = new Cereri
                {
                    Name = model.Name,
                    Description = model.Description,
                    createdOn = DateTime.Now,
                    IsActive = true,
                    CreatedByUser = user,
                    value = model.Value ?? 0,
                    Documente = listaSignaturi,
                    VersionNumber = 1,
                    OldCereri = null
                };

                _context.Cereri.Add(cerere);
            }

            // --- 3. Salvăm întâi cererea (pentru a avea Id!)
            await _context.SaveChangesAsync();

            // Salvează fișierele atașate dacă există
            if (files != null && files.Count > 0)
            {
                string folder = Path.Combine(_fileStoragePath, cerere.Id.ToString());
                Directory.CreateDirectory(folder);

                foreach (var f in files)
                {
                    var filePath = Path.Combine(folder, f.FileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await f.CopyToAsync(stream);

                    cerere.Files.Add(new CerereFile
                    {
                        FileName = f.FileName,
                        FilePath = filePath,
                        CereriId = cerere.Id
                    });
                }
            }

            await _context.SaveChangesAsync();

            return cerere;
        }






        //Reload Cache
        public async Task ReloadCacheAsync(
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
            await _cacheLock.WaitAsync();
            try
            {
                _cachedData = await GetFilteredCereriAsync(
                    sortOrder, pageNumber, pageSize,
                    searchString, filter, nrCrt, description,
                    creat_de, sters_de, data_creare, data_stergere
                );

                _lastLoad = DateTime.UtcNow;
            }
            finally
            {
                _cacheLock.Release();
            }


        }
    }
}
