using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LearningProject.Controllers
{
    [Authorize(Roles = "CereriAcces")]
    public class CereriController : Controller
    {
        private readonly LearningProjectContext _context;

        public CereriController(LearningProjectContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var filterCereri = _context.Cereri.Where(x => x.IsActive == true).Include(c => c.CreatedByUser).Include(c => c.DeletedBy);
            //var learningProjectContext = _context.Cereri.Include(c => c.CreatedByUser).Include(c => c.DeletedBy);
            return View(await filterCereri.ToListAsync());
        }


        public async Task<IActionResult> FilterCereri(string filter = "toate")
        {
            // Definim toate cele trei colecții
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

            // Variabila care va stoca rezultatul final
            IQueryable<Cereri> query;

            // Alegem în funcție de parametru
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

            // Returnăm lista în view
            return View(await query.ToListAsync());
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

        // GET: Cereri/Create
        public IActionResult Create()
        {
            var ADuser = User.Identity.Name.Replace("MMRMAKITA\\", "");
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "IdUser", "Name");
            ViewData["DeletedById"] = new SelectList(_context.User, "IdUser", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNewCerereModel model)
        {
            if (ModelState.IsValid)
            {
                var userNameClaim = User.FindFirstValue(ClaimTypes.Name);
                //var CurrentUserName = CurrentUser.Name.Replace("MMRMAKITA\\", "");
                // int currentUserId = int.Parse(userIdClaim);

                var CurrentUserName = userNameClaim.Replace("MMRMAKITA\\", "");

                var user = await _context.User.FirstOrDefaultAsync(w => w.Name == CurrentUserName);


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
                return RedirectToAction(nameof(Index));

            }
            return View(model);
        }
        public async Task<IActionResult> Edit(int? id)
        {


            if (id == null)
            {
                return NotFound();
            }

            var cereri = await _context.Cereri.FindAsync(id);
            if (cereri == null)
            {
                return NotFound();
            }
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "IdUser", "Name", cereri.CreatedByUserId);
            ViewData["DeletedById"] = new SelectList(_context.User, "IdUser", "Name", cereri.DeletedById);
            return View(cereri);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,createdOn,IsActive,Deleted,CreatedByUserId,DeletedById,value")] Cereri cereri)
        {
            if (id != cereri.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cereri);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CereriExists(cereri.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedByUserId"] = new SelectList(_context.User, "IdUser", "Name", cereri.CreatedByUserId);
            ViewData["DeletedById"] = new SelectList(_context.User, "IdUser", "Name", cereri.DeletedById);
            return View(cereri);
        }

        public async Task<IActionResult> Delete(int? id)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ADuser = User.Identity.Name.Replace("MMRMAKITA\\", "");
            var cereri = await _context.Cereri.FindAsync(id);
            var userNameClaim = User.FindFirstValue(ClaimTypes.Name);


            var CurrentUserName = userNameClaim.Replace("MMRMAKITA\\", "");

            var user = await _context.User.FirstOrDefaultAsync(w => w.Name == CurrentUserName);
            if (cereri != null)
            {
                cereri.IsActive = false;
                cereri.Deleted = DateTime.Now;
                cereri.DeletedBy = user;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CereriExists(int id)
        {
            return _context.Cereri.Any(e => e.Id == id);
        }
    }
}
