using LearningProject.Data;
using LearningProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            var ADuser = User.Identity.Name.Replace("MMRMAKITA\\", "");

            var my_user = await _context.User
                          .Include(c => c.roluri)
                .FirstOrDefaultAsync(u => u.Username == ADuser);

            var current_role = await _context.Roluri
                            .Include(c => c.Users)
                            .Where(w => w.IdRol == my_user.roluriID).FirstAsync();

            var roleName = current_role.Denumire_rol;


            if (my_user == null)
            {
                return NotFound("Utilizatorul nu a fost găsit.");
            }

            var pendingSignatures = await _context.Signatures
                .Where(s =>s.Status == StatusDocument.Nesemnat &&
                    s.ClaimCanSign.name == roleName)
                .Include(s => s.ClaimCanSign)
                 .Include(s => s.Cerere)
                .ToListAsync();

            var allowedToSign = new List<Signature>();


            if (!pendingSignatures.Any())
            {
                return NotFound("Nu există documente nesemnate pentru această cerere.");
            }

            foreach (var signature in pendingSignatures)
            {
                // semnăturile documentului curent
                var allSigsForDocument = await _context.Signatures
                    .Where(s => s.CerereId == signature.CerereId)
                    .ToListAsync();

                // căutăm semnăturile dinaintea noastră (order < al nostru)
                var requiredPrevious = allSigsForDocument
                    .Where(s => s.order < signature.order)
                    .ToList();

                // verificăm dacă toate semnăturile anterioare sunt semnate
                bool allPreviousSigned = requiredPrevious
                    .All(s => s.Status == StatusDocument.Semnat);

                if (allPreviousSigned)
                {
                    allowedToSign.Add(signature);
                }
            }

            return View(allowedToSign);
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
        public ActionResult SignCerere(IFormCollection collection)
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


        public async Task<IActionResult> Sign(int id)   
        {
            var userNameClaim = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(userNameClaim))
                return Unauthorized("Nu există un utilizator activ.");

            var currentUserName = userNameClaim.Replace("MMRMAKITA\\", "");

            var user = await _context.User
                .Include(u => u.roluri)
                .FirstOrDefaultAsync(u => u.Username == currentUserName);

            if (user == null)
                return Unauthorized("Utilizatorul curent nu a fost găsit în baza de date.");


            var signature = await _context.Signatures
                .Include(s => s.ClaimCanSign)
                .Where(s => s.CerereId == id &&
                            s.ClaimCanSign.name == user.roluri.Denumire_rol)
                .FirstOrDefaultAsync();

            if (signature == null)
                return NotFound("Nu există o semnătură asociată rolului tău pentru această cerere.");


            // 3. Verifica if semnaturile anterioare sunt deja semnate
            var previousSignatures = await _context.Signatures
                .Where(s => s.CerereId == id && s.order < signature.order)
                .ToListAsync();

            bool canSign = previousSignatures.All(s => s.Status == StatusDocument.Semnat);

            if (!canSign)
                return BadRequest("Nu poți semna înainte ca rolurile anterioare să semneze.");


            signature.Status = StatusDocument.Semnat;
            signature.DataSemnarii = DateTime.Now;
            signature.SignByUserId = user.IdUser;

            _context.Update(signature);
            await _context.SaveChangesAsync();


            return Ok($"Semnătura pentru cererea {id} a fost înregistrată cu succes.");
        }


        // GET: Componente/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Componente/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit()
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
