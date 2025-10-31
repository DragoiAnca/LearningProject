using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Services;
using LearningProject.Services.Impl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LearningProject.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUsers _usersService;
        private readonly LearningProjectContext _context;

        public UsersController(IUsers usersService, LearningProjectContext context)
        {
            _usersService = usersService;
            _context = context;
        }

        // GET: Users
        [HttpGet("/Users")]
        public async Task<IActionResult> Index()
        {
            var users = await _usersService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _usersService.GetAllUsers();
            return Ok(users);
        }

        // GET: Users/Details/5
        [HttpGet("/Users/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _usersService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: Users/Create
        [HttpGet("/Users/Create")]
        public IActionResult Create()
        {
            ViewData["id_departament"] = new SelectList(_context.Departamente, "id_departamente", "Denumire_departament");
            ViewData["roluriID"] = new SelectList(_context.Roluri, "IdRol", "Denumire_rol");
            return View();
        }

        // POST: Users/Create
        [HttpPost("/Users/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,id_departament")] User user)
        {
            // Citește manual rolul din form
            if (int.TryParse(Request.Form["roluriID"], out int rolId))
                user.roluriID = rolId;

            // Setează Departamentul
            if (user.id_departament != 0)
            {
                user.Departamente = await _context.Departamente
                    .FirstOrDefaultAsync(d => d.id_departamente == user.id_departament);
            }

            // Setează Rolul
            if (user.roluriID != null && user.roluriID != 0)
            {
                user.roluri = await _context.Roluri.FindAsync(user.roluriID);
            }

            if (ModelState.IsValid)
            {
                await _usersService.AddAsync(user);
                return RedirectToAction(nameof(Index));
            }

            ViewData["id_departament"] = new SelectList(_context.Departamente, "id_departamente", "Denumire_departament", user.id_departament);
            ViewData["roluriID"] = new SelectList(_context.Roluri, "IdRol", "Denumire_rol", user.roluriID);
            return View(user);
        }

        // GET: Users/Edit/5
        [HttpGet("/Users/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _usersService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            ViewData["id_departament"] = new SelectList(_context.Departamente, "id_departamente", "Denumire_departament", user.id_departament);
            ViewData["roluriID"] = new SelectList(_context.Roluri, "IdRol", "Denumire_rol", user.roluriID);


            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  User user)
        {
            if (id != user.IdUser)
                return NotFound();

            /*  if (ModelState.IsValid)
              {
                  try
                  {
                      await _usersService.UpdateAsync(user);
                  }
                  catch (DbUpdateConcurrencyException)
                  {
                      var exists = await _usersService.GetByIdAsync(user.IdUser);
                      if (exists == null)
                          return NotFound();

                      throw;
                  }
                  return RedirectToAction(nameof(Index));
              }*/

            if (ModelState.IsValid)
            {
                await _usersService.UpdateAsync(user);
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToList();

                foreach (var error in errors)
                {
                    Console.WriteLine($"Key: {error.Key}");
                    foreach (var e in error.Errors)
                        Console.WriteLine($" - {e.ErrorMessage}");
                }
            }


            ViewData["id_departament"] = new SelectList(_context.Departamente, "id_departamente", "Denumire_departament", user.id_departament);
            ViewData["roluriID"] = new SelectList( _context.Roluri,"IdRol","Denumire_rol",user.roluriID);
            return View(user);
        }

        // GET: Users/Delete/5
        [HttpGet("/Users/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _usersService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost("/Users/Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _usersService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
