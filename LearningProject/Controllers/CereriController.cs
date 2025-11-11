using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Models.ViewModels;
using LearningProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Security.Claims;
using LearningProject.Services.Impl;

namespace LearningProject.Controllers
{
    public class CereriController : Controller
    {
        private readonly LearningProjectContext _context;
        private readonly ICereri _cereriService;

        public CereriController(LearningProjectContext context, ICereri cereriService)
        {
            _context = context;
            _cereriService = cereriService;
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
     [FromQuery] int? pageNumber)
        {
            var pagedCereri = await _cereriService.GetFilteredCereriAsync(
                sortOrder,
                pageNumber ?? 1,
                3,
                searchString
            );

            var viewModel = new ViewModelPaginatedListCereri
            {
                ListaCereriCuPaginatie = pagedCereri.ListaCereriCuPaginatie,
                searchInput = searchString,
                sortOrder = sortOrder,
                pageNumber = pageNumber
            };

            return PartialView("_CereriTablePartial", viewModel);
        }

        [Authorize(Roles = "CereriFiltru")]
        [HttpGet]
        public async Task<IActionResult> FilterCereri(string filter = "toate", bool exportExcel = false, int pageNumber = 1)
        {
            int pageSize = 3; // câte cereri pe pagină
            var cereri = _context.Cereri.AsQueryable();


            var cereriInactive = _context.Cereri
                .Where(x => x.IsActive == false)
                .Include(c => c.CreatedByUser)
                .Include(c => c.DeletedBy);

            var cereriActive = _context.Cereri
                .Where(x => x.IsActive == true)
                .Include(c => c.CreatedByUser)
                .Include(c => c.DeletedBy);

            var toateCererile = _context.Cereri
                .Include(c => c.CreatedByUser)
                .Include(c => c.DeletedBy);

            IQueryable<Cereri> query;

            switch (filter.ToLower())
            {
                case "active":
                    query = cereriActive;
                    break;

                case "inactive":
                    query = cereriInactive;
                    break;

                case "toate":
                default:
                    query = toateCererile;
                    break;
            }

          //  var cereri = await query.ToListAsync();
            var cereriPaginate = await PaginatedList<Cereri>.CreateAsync(
cereri.AsNoTracking(), pageNumber, pageSize);

            if (exportExcel)
            {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("Cereri");

                // Header
                IRow headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("Nr. CRT");
                headerRow.CreateCell(1).SetCellValue("Denumire Cerere");
                headerRow.CreateCell(2).SetCellValue("Descriere");
                headerRow.CreateCell(3).SetCellValue("Creat de");
                headerRow.CreateCell(4).SetCellValue("Data Creare");
                headerRow.CreateCell(5).SetCellValue("Data Ștergere");
                headerRow.CreateCell(6).SetCellValue("Șters de");
                headerRow.CreateCell(7).SetCellValue("Status Cerere");

                // Date
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

            var viewModel = new ViewModelPaginatedListCereri
            {
                ListaCereriCuPaginatie = cereriPaginate,
                searchInput = filter,
                sortOrder = null,
                pageNumber = pageNumber
            };

            return View(viewModel);
        }

        [Authorize(Roles = "CereriDetails")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cereri = await _context.Cereri
                .Include(c => c.CreatedByUser)
                .Include(c => c.DeletedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cereri == null)
            {
                return NotFound();
            }

            return View(cereri);
        }

        [Authorize(Roles = "CereriCreate")]
        public IActionResult Create()
        {
            var ADuser = User.Identity.Name.Replace("MMRMAKITA\\", "");
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "IdUser", "Username");
            ViewData["DeletedById"] = new SelectList(_context.User, "IdUser", "Username");
            return View();
        }

        [Authorize(Roles = "CereriCreate")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateNewCerereModel model)
        {
            if (ModelState.IsValid)
            {
                var userNameClaim = User.FindFirstValue(ClaimTypes.Name);
                var CurrentUserName = userNameClaim.Replace("MMRMAKITA\\", "");
                var user = await _context.User.FirstOrDefaultAsync(w => w.Username == CurrentUserName);

                var cerere = new Cereri
                {
                    Name = model.Name,
                    Description = model.Description,
                    createdOn = DateTime.Now,
                    IsActive = true,
                    CreatedByUser = user,
                    value = (double)model.Value
                };
                _context.Cereri.Add(cerere);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(FilterCereri));
            }
            return View(model);
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



        [Authorize(Roles = "CereriEdit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cereri cereri)
        {
            if (id != cereri.Id)
                return NotFound();

            // ✅ Eliminăm erorile pentru CreatedByUser (nu se trimite din form)
            ModelState.Remove(nameof(cereri.CreatedByUser));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cereri); // folosești direct obiectul din form
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(FilterCereri));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CereriExists(cereri.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Refacem dropdownurile pentru view în caz de eroare
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "IdUser", "Name", cereri.CreatedByUserId);
            ViewData["DeletedById"] = new SelectList(_context.User, "IdUser", "Name", cereri.DeletedById);
            return View(cereri);
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