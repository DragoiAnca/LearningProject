using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Models.ViewModels;

namespace LearningProject.Controllers
{
    public class GestionareDepartamenteController : Controller
    {
        private readonly LearningProjectContext _context;

        public GestionareDepartamenteController(LearningProjectContext context)
        {
            _context = context;
        }

        // GET: GestionareDepartamente
        public async Task<IActionResult> Index()
        {
            return View(await _context.Departamente.ToListAsync());
        }

        // GET: GestionareDepartamente/Details/5
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
                .Include(x =>x.Departamente)
                .Where(x =>x.id_departament == id && x.Departamente.isActive)
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

        public async Task<IActionResult> ViewAddNewUserDepartament(int id)
        {

            //var departament = await _context.Departamente.FirstOrDefaultAsync(d => d.Denumire_departament == denumire_deparatament);

          var users = await _context.User
    .Where(u => u.id_departament != id) 
    .ToListAsync();


            return View(users);
        }

        // GET: GestionareDepartamente/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GestionareDepartamente/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: GestionareDepartamente/Edit/5
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

        // POST: GestionareDepartamente/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: GestionareDepartamente/Delete/5
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

        // POST: GestionareDepartamente/Delete/5
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
