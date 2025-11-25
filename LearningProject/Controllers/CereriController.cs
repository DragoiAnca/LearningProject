using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Models.DraftModel;
using LearningProject.Models.ViewModels;
using LearningProject.Services.Impl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Security.Claims;

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


        //Creare o ciorna by default
        [Authorize(Roles = "CereriCreate")]
        public async Task<IActionResult> Create()
        {
            var ADuser = User.Identity.Name.Replace("MMRMAKITA\\", "");
            var id_user = _context.User.Where(w => w.Username == ADuser).First();

            var draft = await _context.Cereri
                  .Include(c => c.Files)
                  .FirstOrDefaultAsync(c => c.CreatedByUser.Username == ADuser && c.isDraft);

            if (draft == null)
            {
                // Creează draft gol
                draft = new Cereri
                {
                    CreatedByUserId = id_user.IdUser,
                    createdOn = DateTime.Now,
                    isDraft = true,
                    Name = "",           
                    Description = "",     
                    value = 0 
                };
                _context.Cereri.Add(draft);
                await _context.SaveChangesAsync();
            }

            var model = new CreateNewCerereModel
            {
                Name = draft.Name,
                Description = draft.Description,
                Value = draft.value,
                DraftId = draft.Id,
                ExistingFiles = draft.Files.ToList() 
             };

            ViewData["CreatedByUserId"] = new SelectList(_context.User, "IdUser", "Username");
            ViewData["DeletedById"] = new SelectList(_context.User, "IdUser", "Username");

            return View(model);
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


        //se ocupa de finalizarea cereri 
        [HttpPost]
        [Authorize(Roles = "CereriCreate")]
        public async Task<IActionResult> Create(CreateNewCerereModel model)
        {
            if (!ModelState.IsValid) 
                return View(model);

            var username = User.Identity.Name.Replace("MMRMAKITA\\", "");

            if (username == null) 
                throw new Exception("User not found.");


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
                .Include(d => d.Files)
                .FirstOrDefaultAsync(d => d.Id == model.DraftId && d.isDraft);

            if (draft != null)
            {
                model.Name ??= draft.Name;
                model.Description ??= draft.Description;
                model.Value ??= draft.value;
            }

            var cerere = await _cereriServiceCreate.CreateCerereAsync(model, username);

            //cititre fisier 
            //if (draft?.Files != null && draft.Files.Any())
            //{
            //    string finalFolder = Path.Combine(_fileStoragePath, cerere.Id.ToString());
            //    Directory.CreateDirectory(finalFolder);

            //    foreach (var f in draft.Files)
            //    {
            //        var newPath = Path.Combine(finalFolder, f.FileName);
            //        System.IO.File.Move(f.FilePath, newPath);

            //        f.FilePath = newPath;
            //    }
            //}

            //// Șterge draftul
            //if (draft != null)
            //{
            //    _context.Cereri.Remove(draft);
            //    await _context.SaveChangesAsync();
            //}

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
        public async Task<IActionResult> CerereStearsa(int id,IFormFile asd)
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
                });
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


        //se ocupa doar de actualizarea campurilor
        [HttpPut]
        public async Task<IActionResult> SaveDraft([FromForm] CreateNewCerereModel dto)
        {
            Cereri draft;
            CerereFile dto_file;

            if (dto.DraftId == 0)
            {
                // Creează draft nou
                var username = User.Identity.Name.Replace("MMRMAKITA\\", "");
                var user = _context.User.First(u => u.Username == username);

                draft = new Cereri
                {
                    CreatedByUserId = user.IdUser,
                    createdOn = DateTime.Now,
                    isDraft = true
                };
                _context.Cereri.Add(draft);
                await _context.SaveChangesAsync();
            }
            else
            {
                draft = await _context.Cereri
                    .Include(d => d.Files)
                    .FirstOrDefaultAsync(d => d.Id == dto.DraftId);

                if (draft == null) return NotFound("Draft not found.");
            }

            // Actualizează câmpuri
            if (!string.IsNullOrEmpty(dto.Name)) draft.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) draft.Description = dto.Description;
            if (dto.Value.HasValue) draft.value = dto.Value.Value;


            // Salvează fisiere și pregătește lista pentru JS
            var uploadedFilesList = new List<object>();

            if (dto.UploadedFiles != null && dto.UploadedFiles.Count > 0)
            {
                string draftFolder = Path.Combine(_fileStoragePath, draft.Id.ToString());
                Directory.CreateDirectory(draftFolder);

                foreach (var uploadedFile in dto.UploadedFiles)
                {
                    var filePath = Path.Combine(draftFolder, uploadedFile.FileName);

                    // 🔹 Verificăm dacă există deja în draft.Files
                    bool fileExists = draft.Files.Any(f =>
                        string.Equals(f.FileName, uploadedFile.FileName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(f.FilePath, filePath, StringComparison.OrdinalIgnoreCase)
                    );

                    if (fileExists)
                    {


                        uploadedFilesList.Add(new
                        {
                            FileName = uploadedFile.FileName,
                            FilePath = $"/uploads/{draft.Id}/{uploadedFile.FileName}",
                            AlreadyExists = true
                        });


                        // Dacă fișierul există deja, sărim peste el
                        continue;
                    }

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await uploadedFile.CopyToAsync(stream);


                    // 🔹 Creăm CerereFile și îl adăugăm în draft

                    var cerereFile = new CerereFile
                    {
                        FileName = uploadedFile.FileName,
                        FilePath = filePath,
                        CereriId = draft.Id
                    };
                    draft.Files.Add(cerereFile);

                    // Pregătim obiect pentru JS
                    uploadedFilesList.Add(new
                    {
                        FileName = cerereFile.FileName,
                        FilePath = $"/uploads/{draft.Id}/{cerereFile.FileName}",// sau ruta accesibilă public
                        AlreadyExists = false
                    });
                }
            }

            draft.isDraft = true;
            await _context.SaveChangesAsync();

            // Returnăm DraftId și lista fișierelor pentru JS
            return Ok(new
            {
                DraftId = draft.Id,
                Files = uploadedFilesList
            });
        }



        //sterge in functie de id

        public ActionResult DeleteByID( int id)
        {
            CerereFile dto_file;

            var current_cerere = _context.CerereFile.Where(w => w.Id == id).First();

            if (current_cerere == null)
            {
                return NotFound();
            }
            _context.CerereFile.Remove(current_cerere);
            _context.SaveChanges();
            return RedirectToAction("Create"); 
        }


        //sterge toate randurile in functie de id_cerere
        public ActionResult DeleteAllCereri(int idCerere)
        {
            var filesToDelete = _context.CerereFile
                               .Where(f => f.CereriId == idCerere)
                               .ToList();

            //if (!filesToDelete.Any())
            //{
            //    return Json(new { success = false, message = "Cererea nu a fost găsită." });
            //}

            // Ștergem toate înregistrările găsite
            _context.CerereFile.RemoveRange(filesToDelete);

            _context.SaveChanges();

            return RedirectToAction("Create"); 
             }
        }
}