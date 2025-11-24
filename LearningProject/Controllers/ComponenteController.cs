using LearningProject.Data;
using LearningProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningProject.Controllers
{
    public class ComponenteController : Controller
    {
        private readonly LearningProjectContext _context;

        public ComponenteController(LearningProjectContext context)
        {
            _context = context;
        }

        // GET: Componente
        public async Task<ActionResult> Index()
        {
            // 1. Obține username-ul din AD
            var ADuser = User.Identity.Name.Replace("MMRMAKITA\\", "");

            // 2. Obține utilizatorul din baza de date
            var my_user = await _context.User
                          .Include(c => c.roluri)
                .FirstOrDefaultAsync(u => u.Username == ADuser);

            var current_role = await _context.Roluri
                            .Include(c => c.Users)
                            .Where(w => w.IdRol == my_user.roluriID).FirstAsync();

            if (my_user == null)
            {
                return NotFound("Utilizatorul nu a fost găsit.");
            }

            // 3. Adună toate semnăturile care sunt "Nesemnat"
            var pendingSignatures = await _context.Signatures
                .Where(s =>s.Status == StatusDocument.Nesemnat)
                .Include(s => s.ClaimCanSign) // includem claim-ul pentru verificarea rolului
                .ToListAsync();

            if (!pendingSignatures.Any())
            {
                return NotFound("Nu există documente nesemnate pentru această cerere.");
            }


            // 4. Verifică dacă utilizatorul are rolul care îi permite să semneze
            var canSignList = pendingSignatures
                .Where(s => s.ClaimCanSign != null && s.ClaimCanSign.name.Equals(current_role.Denumire_rol))
                .ToList();

            return View(canSignList);
        }

        // GET: Componente/Details/5
        public async Task<ActionResult> Details(int id)
        {

            //adu username
            // 1. Obține username-ul din AD
            var ADuser = User.Identity.Name.Replace("MMRMAKITA\\", "");

            // 2. Obține utilizatorul din baza de date
            var my_user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == ADuser);

            if (my_user == null)
            {
                return NotFound("Utilizatorul nu a fost găsit.");
            }

            // 3. Adună toate semnăturile din cererea cu id-ul respectiv, care sunt "Nesemnat"
            var pendingSignatures = await _context.Signatures
                .Where(s => s.CerereId == id && s.Status == StatusDocument.Nesemnat)
                .Include(s => s.ClaimCanSign) // includem claim-ul pentru verificarea rolului
                .ToListAsync();

            if (!pendingSignatures.Any())
            {
                return NotFound("Nu există documente nesemnate pentru această cerere.");
            }

            // 4. Verifică dacă utilizatorul are rolul care îi permite să semneze
            var canSignList = pendingSignatures
                .Where(s => s.ClaimCanSign != null && s.ClaimCanSign.name == my_user.Username)
                .ToList();

            return View(canSignList);
        }

        // GET: Componente/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Componente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Componente/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Componente/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Componente/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Componente/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
