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
using System.Threading.Tasks;

namespace LearningProject.Controllers
{
    public class GestionareDepartamenteController : Controller
    {
        private readonly LearningProjectContext _context;

        public GestionareDepartamenteController(LearningProjectContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "GestionareDepartamenteIndex")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Departamente.ToListAsync());
        }

        [Authorize(Roles = "GestionareDepartamenteDetails")]
        public async Task<IActionResult> Details(int? id)
        {
            //Option1
            //var users =await _context.User.Where(m => m.id_departament == id).ToListAsync();
            //Option2

            var users = _context.User
    .Where(m => m.id_departament == id)
    .Select(z => new UserDepartmentSorter { Name = z.Name, departamentID = z.id_departament })
    .ToList();

            var activeUsers = _context.User
                .Include(x => x.Departamente)
                .Where(x => x.id_departament == id && x.Departamente.isActive)
                .Select(z => new UserDepartmentSorter { Name = z.Name })
                .ToList();


            //var activeUsers = await _context.User.Include(x => x.Departamente).Where(x => x.id_departament == id && x.Departamente.isActive).ToListAsync();

            var departament = await _context.Departamente.FirstOrDefaultAsync(d => d.id_departamente == id);
            ViewData["DepartamentName"] = departament.Denumire_departament;
            ViewData["DepartamentID"] = departament.id_departamente;

            if (users == null || !users.Any())
            {
                ViewData["DepartamentName"] = departament.Denumire_departament;
                return View("NoUsers");
            }

            return View(activeUsers);
        }

        [Authorize(Roles = "GestionareDepartamenteViewAddNewUserDepartament")]
        public async Task<IActionResult> ViewAddNewUserDepartament(int id)
        {

            //var departament = await _context.Departamente.FirstOrDefaultAsync(d => d.Denumire_departament == denumire_deparatament);

            var users = await _context.User
      .Where(u => u.id_departament != id)
      .ToListAsync();


            return View(users);
        }

        [Authorize(Roles = "GestionareDepartamenteCreate")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "GestionareDepartamenteCreate")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id_departamente,Denumire_departament,isActive,data_inceput,data_dezactivare")] Departamente departamente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(departamente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(departamente);
        }

        [Authorize(Roles = "GestionareDepartamenteEdit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var departamente = await _context.Departamente.FindAsync(id);
            if (departamente == null)
            {
                return NotFound();
            }
            return View(departamente);
        }

        [Authorize(Roles = "GestionareDepartamenteEdit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id_departamente,Denumire_departament,isActive,data_inceput,data_dezactivare")] Departamente departamente)
        {
            if (id != departamente.id_departamente)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(departamente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartamenteExists(departamente.id_departamente))
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
            return View(departamente);
        }

        [Authorize(Roles = "GestionareDepartamenteDelete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var departamente = await _context.Departamente
                .FirstOrDefaultAsync(m => m.id_departamente == id);
            if (departamente == null)
            {
                return NotFound();
            }

            return View(departamente);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var departamente = await _context.Departamente.FindAsync(id);
            if (departamente != null)
            {
                _context.Departamente.Remove(departamente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DepartamenteExists(int id)
        {
            return _context.Departamente.Any(e => e.id_departamente == id);
        }
    }
}
