using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Security.Claims;
using LearningProject.Services.Impl;
using System.Net;





namespace LearningProject.Controllers
{
    public class CereriController : Controller
    {
        private readonly LearningProjectContext _context;
        private readonly ICereriCacheService _cereriService;
        private readonly ICereri _cereriServiceCreate;
        private readonly IBufferedFileUploadService _bufferedFileUploadService;

        private readonly string _fileStoragePath;

        public CereriController(
            LearningProjectContext context,
            ICereriCacheService cereriService,
            ICereri cereriServiceCreate,
            IBufferedFileUploadService bufferedFileUploadService,
            IConfiguration configuration)
        {
            _bufferedFileUploadService = bufferedFileUploadService;
            _context = context;
            _cereriService = cereriService;
            _cereriServiceCreate = cereriServiceCreate;
            _fileStoragePath = configuration["ApiUrls:FileStoragePath"];
        }
        //public async Task<IActionResult> UploadFileCompletedEventArgs (IFormFile Fisier)
        //{
        //    return Ok(Fisier.Name);
        //} 



        //search field - autofill
        [HttpGet("/GetAutofill")]
        public async Task<IActionResult> GetAutofill(string autocompleteText)
        {
            var query = _context.Cereri.AsQueryable();

            // filtrare doar dupa denumire_cerere
            if (!string.IsNullOrEmpty(autocompleteText))
            {
                string lower = autocompleteText.ToLower();
                query = query
                      .Where(e => e.Name != null && e.Name.ToLower().StartsWith(lower) 
                       );
            }

            var name_cerere = await query
                .Select(e => new { e.Name}).Distinct()
                .ToListAsync();

            return Ok(name_cerere);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVersions(int id)
        {
            // 1️. găsim cererea pornind de la id
            var current = await _context.Cereri
                .Include(c => c.Documente)
                .Include(c => c.CreatedByUser)
                .Include(c => c.DeletedBy)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (current == null)
                return NotFound("Cererea nu există.");

            // 2️⃣ mergem înapoi până la root
            var root = current;
            while (root.OldCereriId.HasValue)
            {
                root = await _context.Cereri
                    .FirstOrDefaultAsync(c => c.Id == root.OldCereriId.Value);
            }

            // 3️⃣ mergem înainte și generăm lanțul complet
            var versions = new List<Cereri>();
            var node = root;

            while (node != null)
            {
                versions.Add(node);

                node = await _context.Cereri
                    .FirstOrDefaultAsync(c => c.OldCereriId == node.Id);
            }


            // 4️⃣ trimitem către client datele formate
            var result = versions.Select(v => new
            {
                v.Id,
                v.Name,
                v.Description,
                v.createdOn,
                v.value,
                Version = v.VersionNumber,
                CreatedBy = v.CreatedByUser?.Username,
                DeletedBy = v.DeletedBy?.Username,
                NrDocumente = v.Documente.Count,
            });

            return Json(result);
        }



        [Authorize(Roles = "CereriIndex")]
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            var students = from s in _context.Students
                           select s;
            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }
            return View(await students.AsNoTracking().ToListAsync());
        }

        public async Task<IActionResult> FilterCereriPartial(
     [FromQuery(Name = "searchString")] string? searchString,
     [FromQuery] string? sortOrder,
     [FromQuery] int? pageNumber,
     [FromQuery] string? filter,
     [FromQuery] int? nrCrt,
     [FromQuery] string? description,
     [FromQuery] string? creat_de,
     [FromQuery] string? sters_de,
     [FromQuery] DateTime? data_creare,
     [FromQuery] DateTime? data_stergere)
        {
            var pagedCereri = await _cereriServiceCreate.GetFilteredCereriAsync(
                sortOrder,
                pageNumber ?? 1,
                3,
                searchString,
                filter,
                nrCrt,
                description,
                creat_de,
                sters_de,
                data_creare,
                data_stergere
            );

            var viewModel = new ViewModelPaginatedListCereri
            {
                ListaCereriCuPaginatie = pagedCereri.ListaCereriCuPaginatie,
                searchInput = searchString,
                sortOrder = sortOrder,
                pageNumber = pageNumber,
                filter_value = filter,
                nrCrt = nrCrt,
                descriere_value = description,
                creat_de = creat_de,
                sters_de = sters_de,
                data_creare = data_creare,
                data_stergere = data_stergere
            };

            return PartialView("_CereriTablePartial", viewModel);
        }

        [Authorize(Roles = "CereriFiltru")]
        [HttpGet]
        public async Task<IActionResult> FilterCereri(string filter = "toate", bool exportExcel = false, int pageNumber = 1)
        {
            int pageSize = 3;
            var cereri = _context.Cereri
                .Include(c => c.CreatedByUser)
                .Include(c => c.DeletedBy)
                .AsQueryable();

            // 🔸 Aplicăm filtrul
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
                    // fără filtrare
                    break;
            }

            // 🔸 Sortează descrescător după Id
            cereri = cereri.OrderByDescending(c => c.Id);

            // 🔹 Paginare
            var cereriPaginate = await PaginatedList<Cereri>.CreateAsync(
                cereri.AsNoTracking(), pageNumber, pageSize);

            // 🔹 Export Excel
            if (exportExcel)
            {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("Cereri");

                IRow headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Nr. CRT");
                headerRow.CreateCell(1).SetCellValue("Denumire Cerere");
                headerRow.CreateCell(2).SetCellValue("Descriere");
                headerRow.CreateCell(3).SetCellValue("Creat de");
                headerRow.CreateCell(4).SetCellValue("Data Creare");
                headerRow.CreateCell(5).SetCellValue("Data Ștergere");
                headerRow.CreateCell(6).SetCellValue("Șters de");
                headerRow.CreateCell(7).SetCellValue("Status Cerere");

                int rowIndex = 1;
                foreach (var c in cereri)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(rowIndex - 1);
                    row.CreateCell(1).SetCellValue(c.Name ?? "");
                    row.CreateCell(2).SetCellValue(c.Description ?? "");
                    row.CreateCell(3).SetCellValue(c.CreatedByUser?.Username ?? "");
                    row.CreateCell(4).SetCellValue(c.createdOn.ToString("yyyy-MM-dd HH:mm"));
                    row.CreateCell(5).SetCellValue(c.Deleted?.ToString("yyyy-MM-dd HH:mm") ?? "");
                    row.CreateCell(6).SetCellValue(c.DeletedBy?.Username ?? "");
                    row.CreateCell(7).SetCellValue(c.IsActive ? "Activă" : "Inactivă");
                }

                for (int i = 0; i < 8; i++)
                    sheet.AutoSizeColumn(i);

                using var exportData = new MemoryStream();
                workbook.Write(exportData);
                workbook.Close();

                string fileName = $"Cereri_{filter}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(exportData.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }

            // 🔹 Construim ViewModel-ul
            var viewModel = new ViewModelPaginatedListCereri
            {
                ListaCereriCuPaginatie = cereriPaginate,
                filter_value = filter,
                pageNumber = pageNumber
            };

            // 🔹 Populăm opțiunile pentru dropdown
            ViewBag.FilterOptions = new List<SelectListItem>
    {
        new SelectListItem { Text = "Toate", Value = "toate", Selected = (filter == "toate") },
        new SelectListItem { Text = "Active", Value = "active", Selected = (filter == "active") },
        new SelectListItem { Text = "Inactive", Value = "inactive", Selected = (filter == "inactive") }
    };

            return View(viewModel);
        }

        [Authorize(Roles = "CereriDetails")]
        public async Task<IActionResult> Details(int? id)
        {
            var cerere = _context.Cereri
         .Include(c => c.CreatedByUser)
         .Include(c => c.DeletedBy)
         .Include(c => c.Documente) // Include semnăturile
         .ThenInclude(s => s.SignByUser)
         .Include(c => c.Documente)
         .ThenInclude(s => s.ClaimCanSign)
         .FirstOrDefault(c => c.Id == id);

            if (cerere == null) return NotFound();

            return View(cerere);
        }

        [Authorize(Roles = "CereriCreate")]
        public IActionResult Create()
        {
            var ADuser = User.Identity.Name.Replace("MMRMAKITA\\", "");
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "IdUser", "Username");
            ViewData["DeletedById"] = new SelectList(_context.User, "IdUser", "Username");
            return View();
        }

       // [Authorize(Roles = "Checker,Manager")]
        [HttpPost]
        public async Task<IActionResult> SignCerere(int cerereId, int signatureId, bool sign)
        {
            var userNameClaim = User.FindFirstValue(ClaimTypes.Name);
            var currentUserName = userNameClaim.Replace("MMRMAKITA\\", "");
            var user = await _context.User.FirstOrDefaultAsync(u => u.Username == currentUserName);

            if (user == null)
                return Unauthorized("Utilizatorul curent nu a fost găsit în baza de date.");

            var signature = await _context.Signatures
                .Include(s => s.Cerere)
                .Include(s => s.ClaimCanSign)
                .FirstOrDefaultAsync(s => s.Id == signatureId && s.CerereId == cerereId);

            if (signature == null)
                return NotFound("Semnătura nu a fost găsită pentru această cerere.");

            signature.Status = sign ? StatusDocument.Semnat : StatusDocument.Refuzat;
            signature.DataSemnarii = DateTime.Now;
            signature.SignByUserId = user.IdUser;

            _context.Update(signature);
            await _context.SaveChangesAsync();


            return RedirectToAction("FilterCereri");
        }


        [Authorize(Roles = "CereriCreate")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateNewCerereModel model)// description, name, value
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentUserName = User.FindFirstValue(ClaimTypes.Name)
                                      .Replace("MMRMAKITA\\", "");

            // 1️. Creeaza cererea
            var cerere = await _cereriServiceCreate.CreateCerereAsync(model, currentUserName);

            // 2. Upload fișiere dacă există
            if (model.UploadedFiles != null && model.UploadedFiles.Any())
            {
                var cererePath = Path.Combine(_fileStoragePath, cerere.Id.ToString());

                foreach (var file in model.UploadedFiles)
                {
                    await _bufferedFileUploadService.UploadFile(file, cererePath);

                    cerere.Files.Add(new CerereFile
                    {
                        FileName = file.FileName,
                        FilePath = Path.Combine(cererePath, file.FileName)
                    });
                }


                //Codul actual dat de Chatty
                //aici filtreaza, sorteaza, nu creaza o cerere noua...
                //await _cereriServiceCreate.GetFilteredCereriAsync(cerere);

                //Codul vechi
               // var cerere = await _cereriServiceCreate.CreateCerereAsync(model, currentUserName);

            }

            return RedirectToAction("FilterCereri");
        }

        public IActionResult FilterCereriBy(string filter)
        {
            IEnumerable<Cereri> cereri = _context.Cereri.AsEnumerable();

            switch (filter)
            {
                case "active":
                    cereri = cereri.Where(c => c.IsActive);
                    break;
                case "inactive":
                    cereri = cereri.Where(c => !c.IsActive);
                    break;
                default:
                    break;
            }

            return PartialView("_CereriTablePartial", cereri);
        }

        [Authorize(Roles = "CereriEdit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cereri = await _context.Cereri.Where(m=>m.Id==id && m.IsActive==true).FirstOrDefaultAsync();
            if (cereri == null)
            {
                return NotFound();
            }
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "IdUser", "Name", cereri.CreatedByUserId);
            ViewData["DeletedById"] = new SelectList(_context.User, "IdUser", "Name", cereri.DeletedById);
            return View(cereri);
        }


        // Edit 
        // Edit / Update cu versiuni
        [Authorize(Roles = "CereriEdit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cereri model)
        {
            if (id != model.Id)
                return Json(new { success = false, message = "ID invalid" });

            // Eliminăm erorile legate de navigational properties care nu vin din form
            ModelState.Remove(nameof(model.CreatedByUser));
            ModelState.Remove(nameof(model.DeletedBy));

            // Validare model
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                             .Where(ms => ms.Value.Errors.Any())
                             .ToDictionary(
                                 kv => kv.Key.Replace("model.", ""), // numele câmpului din form
                                 kv => kv.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                             );

                return Json(new { success = false, errors });
            }

            // Preluăm cererea existentă din DB
            var oldCerere = await _context.Cereri
                                          .Include(c => c.Documente)
                                          .FirstOrDefaultAsync(c => c.Id == id);

            if (oldCerere == null)
                return Json(new { success = false, message = "Cererea nu a fost găsită." });

            try
            {
                //Daca e semnata creeaza versiune noua 
                if (oldCerere.Documente.All(d => d.Status == StatusDocument.Semnat))
                {
                    var newCerere = new Cereri
                    {
                        Name = model.Name,
                        Description = model.Description,
                        createdOn = DateTime.Now,
                        IsActive = true,
                        CreatedByUserId = oldCerere.CreatedByUserId,
                        value = model.value,
                        OldCereri = oldCerere,                
                        VersionNumber = (oldCerere.VersionNumber ?? 1) + 1,
                        Documente = oldCerere.Documente.Select(d => new Signature
                        {
                            ClaimCanSignId = d.ClaimCanSignId,
                            Status = d.Status,   
                            order = d.order
                        }).ToList()
                    };

                    _context.Cereri.Add(newCerere);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        message = $"Cererea a fost actualizată. Versiunea curentă: {newCerere.VersionNumber}"
                    });
                }
                else
                {
                    // Cerere NESemnată → update direct
                    oldCerere.Name = model.Name;
                    oldCerere.Description = model.Description;
                    oldCerere.value = model.value;

                    // OldCereri rămâne null, VersionNumber rămâne 1
                    _context.Update(oldCerere);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Cererea a fost actualizată." });
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CereriExists(model.Id))
                    return Json(new { success = false, message = "Cererea nu există." });
                else
                    throw;
            }
        }


        //varianta updata de edit 
        [Authorize(Roles = "CereriEdit")]
        [HttpPost]
        public async Task<IActionResult> EditVersioned(int id, Cereri model)
        {
            var oldCerere = await _context.Cereri
                                          .Include(c => c.Documente)
                                          .FirstOrDefaultAsync(c => c.Id == id);

            if (oldCerere == null) return NotFound("Cererea nu a fost găsită.");

            var newCerere = new Cereri
            {
                Name = model.Name,
                Description = model.Description,
                createdOn = DateTime.Now,
                IsActive = true,
                CreatedByUserId = oldCerere.CreatedByUserId,
                value = model.value,
                OldCereri = oldCerere,
                VersionNumber = (oldCerere.VersionNumber ?? 1) + 1,  // 2, 3, ...
            };

          

            _context.Cereri.Add(newCerere);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Cererea a fost actualizată. Versiunea curentă: {newCerere.VersionNumber}" });
        }


        [Authorize(Roles = "CereriDelete")]
        public async Task<IActionResult> GetDeleteModal(int id)
        {
            var cereri = await _context.Cereri
                .Include(c => c.CreatedByUser)
                .Include(c => c.DeletedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cereri == null)
            {
                return NotFound();
            }

            return PartialView("_DeleteModalPartial", cereri);
        }

       [Authorize(Roles = "CereriEdit")]
        public async Task<IActionResult> GetEditModal(int id)
        {
            var cereri = await _context.Cereri
                .Include(c => c.CreatedByUser)
                .Include(c => c.DeletedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cereri == null)
            {
                return NotFound();
            }

            ViewData["CreatedByUserId"] = new SelectList(_context.User, "IdUser", "Name", cereri.CreatedByUserId);
            ViewData["DeletedById"] = new SelectList(_context.User, "IdUser", "Name", cereri.DeletedById);

            return PartialView("_EditModalPartial", cereri);
        }



        //[Authorize(Roles = "CereriEdit")]
        public async Task<IActionResult> GetDetaliiModal(int id)
        {
            var cereri = await _context.Cereri
                  .Include(c => c.CreatedByUser)                 
                  .Include(c => c.DeletedBy)                      
                  .Include(c => c.Documente)                    
                      .ThenInclude(d => d.ClaimCanSign)         
                  .Include(c => c.Documente)
                      .ThenInclude(d => d.SignByUser)           
                  .FirstOrDefaultAsync(m => m.Id == id);

            if (cereri == null)
            {
                return NotFound();
            }

            ViewData["CreatedByUserId"] = new SelectList(_context.User, "IdUser", "Name", cereri.CreatedByUserId);
            ViewData["DeletedById"] = new SelectList(_context.User, "IdUser", "Name", cereri.DeletedById);

            return PartialView("_DetaliiModalPartial", cereri);
        }

        [Authorize(Roles = "CereriDelete")]
        [HttpPost]
       // [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cereri = await _context.Cereri.FindAsync(id);
            var userNameClaim = User.FindFirstValue(ClaimTypes.Name);
            var CurrentUserName = userNameClaim.Replace("MMRMAKITA\\", "");
            var user = await _context.User.FirstOrDefaultAsync(w => w.Username == CurrentUserName);

            if (cereri != null)
            {
                cereri.IsActive = false;
                cereri.Deleted = DateTime.Now;
                cereri.DeletedBy = user;
                await _context.SaveChangesAsync();
            }

            // 👉 în loc de RedirectToAction
            return Ok(new
            {
                success = true,
                message = "Cererea a fost dezactivată cu succes."
            });
        }


        [Authorize(Roles = "CereriDelete")]
        [HttpPost]
        // [ValidateAntiForgeryToken] // Poți reactiva dacă folosești token-ul
        public async Task<IActionResult> CerereStearsa(int id)
        {
            try
            {
                var cerere = await _context.Cereri
                    .Include(c => c.DeletedBy) // poți elimina dacă nu mai folosești DeletedBy
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cerere == null)
                {
                    return NotFound(new { success = false, message = "Cererea nu a fost găsită." });
                }

                _context.Cereri.Remove(cerere);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Cererea cu ID {id} a fost ștearsă cu succes.",
                    redirectUrl = Url.Action("FilterCereri")
                });;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Eroare la ștergere: {ex.Message}" });
            }
        }

        private bool CereriExists(int id)
        {
            return _context.Cereri.Any(e => e.Id == id);
        }


       

    }
}