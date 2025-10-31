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
    public class RoluriController : Controller
    {
        private readonly LearningProjectContext _context;

        public RoluriController(LearningProjectContext context)
        {
            _context = context;
        }

        // GET: Roluri
       // [Authorize(Roles = "Junior")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Roluri.ToListAsync());
        }

        // GET: Roluri/Details/5

        //[Authorize(Roles = "Junior")]


        // GET: Roluri/AssignClaims/5
        public async Task<IActionResult> AssignClaims(int id)
        {
            var role = await _context.Roluri
                .Include(r => r.Claims)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (role == null) return NotFound();

            var allClaims = await _context.Claim.ToListAsync();

            var vm = new RoleClaimsViewModel
            {
                RolId = role.IdRol,
                RolDenumire = role.Denumire_rol,
                Claims = allClaims.Select(c => new ClaimCheckbox
                {
                    ClaimId = c.IdClaim,
                    ClaimName = c.name,
                    Selected = role.Claims.Any(rc => rc.IdClaim == c.IdClaim)
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignClaims(RoleClaimsViewModel vm)
        {
            var role = await _context.Roluri
                .Include(r => r.Claims)
                .FirstOrDefaultAsync(r => r.IdRol == vm.RolId);

            if (role == null) return NotFound();

            // Curățăm claims existente
            role.Claims.Clear();

            // Adăugăm claims selectate
            foreach (var c in vm.Claims.Where(c => c.Selected))
            {
                var claim = await _context.Claim.FindAsync(c.ClaimId);
                if (claim != null)
                    role.Claims.Add(claim);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roluri = await _context.Roluri
                .FirstOrDefaultAsync(m => m.IdRol == id);
            if (roluri == null)
            {
                return NotFound();
            }

            return View(roluri);
        }

        // GET: Roluri/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roluri/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRol,Denumire_rol")] Roluri roluri)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roluri);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(roluri);
        }

        // GET: Roluri/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roluri = await _context.Roluri.FindAsync(id);
            if (roluri == null)
            {
                return NotFound();
            }
            return View(roluri);
        }

        // POST: Roluri/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRol,Denumire_rol")] Roluri roluri)
        {
            if (id != roluri.IdRol)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roluri);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoluriExists(roluri.IdRol))
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
            return View(roluri);
        }

        // GET: Roluri/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roluri = await _context.Roluri
                .FirstOrDefaultAsync(m => m.IdRol == id);
            if (roluri == null)
            {
                return NotFound();
            }

            return View(roluri);
        }

        // POST: Roluri/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roluri = await _context.Roluri.FindAsync(id);
            if (roluri != null)
            {
                _context.Roluri.Remove(roluri);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoluriExists(int id)
        {
            return _context.Roluri.Any(e => e.IdRol == id);
        }
    }
}
