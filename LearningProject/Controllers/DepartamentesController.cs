using LearningProject.Data;
using LearningProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LearningProject.Controllers
{
    public class DepartamentesController : Controller
    {
        private readonly LearningProjectContext _context;

        public DepartamentesController(LearningProjectContext context)
        {
            _context = context;
        }

        // GET: Departamentes

       // [Authorize(Roles = "Tester, Developer, Call center")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Departamente.ToListAsync());
        }

        // GET: Departamentes/Details/5
       // [Authorize(Roles = "Manager")]
        //[Authorize(Roles = "Tester")]
        public async Task<IActionResult> Details(int? id)
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

        // GET: Departamentes/Create
        //[Authorize(Roles = "Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departamentes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id_departamente,Denumire_departament")] Departamente departamente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(departamente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(departamente);
        }



        // GET: Departamentes/Edit/5
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

        // POST: Departamentes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id_departamente,Denumire_departament")] Departamente departamente)
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

        // GET: Departamentes/Delete/5
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

        // POST: Departamentes/Delete/5
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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
