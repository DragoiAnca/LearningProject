using Azure.Identity;
using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Services;
using LearningProject.Services.Impl;
using Microsoft.AspNetCore.Authorization;
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

        // GET: Users/ViewTest
        [HttpGet("/Users/ViewTest")]
        public async Task<IActionResult> ViewTest()
        {
            var employees = await _context.V_employees.ToListAsync();
            return Ok(employees);
        }

        // GET: Users
        [Authorize(Roles = "UserIndex")]
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
        [Authorize(Roles = "UserDetails")]
        [HttpGet("/Users/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _usersService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: Users/Create
        [Authorize(Roles = "UserCreate")]
        [HttpGet("/Users/Create")]
        public IActionResult Create()
        {
            ViewData["id_departament"] = new SelectList(_context.Departamente, "id_departamente", "Denumire_departament");
            ViewData["roluriID"] = new SelectList(_context.Roluri, "IdRol", "Denumire_rol");
            return View();
        }


        // POST: Users/Create
        [Authorize(Roles = "UserCreate")]
        [HttpPost("/Users/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            // Citește manual rolul din form
            if (int.TryParse(Request.Form["roluriID"], out int rolId))
                user.roluriID = rolId;


            var my_user = await _context.User
                    .FirstOrDefaultAsync(d => d.Username == user.Username);
            if (my_user != null)
            {
                return BadRequest("Username already exists.");
            }

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

        [Authorize(Roles = "UserGetTest")]
        [HttpGet("/test")]

        public async Task<IActionResult> GetTest(string autocompleteText)
        {
            var query = _context.V_employees.AsQueryable();


            // filtrare doar dupa nume
            if (!string.IsNullOrEmpty(autocompleteText))
            {
                string lower = autocompleteText.ToLower();
                query = query
                      .Where(e => e.FullName != null && e.FullName.ToLower().StartsWith(lower) ||
                       e.EmployeeID != null && e.EmployeeID.ToLower().StartsWith(lower) ||
                       e.Username != null && e.Username.ToLower().StartsWith(lower)
                       );
            }

            var employees = await query
                .Select(e => new { e.EmployeeID, e.FullName, e.Username }).Take(25)
                .ToListAsync();

            return Ok(employees);
        }

        [Authorize(Roles = "UserGetDetails")]
        [HttpGet("/details")]
        public async Task<IActionResult> GetDetails(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest();

            var employee = await _context.V_employees
                .Where(e => e.Username == username)
                .Select(e => new
                {
                    e.Username,
                    e.EmployeeID,//marca
                    e.FullName,
                    e.Email,
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }


        // GET: Users/Edit/5
        [Authorize(Roles = "UserEdit")]
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

        [Authorize(Roles ="UserEdit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
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
            ViewData["roluriID"] = new SelectList(_context.Roluri, "IdRol", "Denumire_rol", user.roluriID);
            return View(user);
        }

        // GET: Users/Delete/5

        [Authorize(Roles ="UserDelete")]
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
