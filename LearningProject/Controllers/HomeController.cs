using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LearningProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LearningProjectContext _context;
        private readonly ErrorLoggerService _errorLogger;

        public HomeController(ILogger<HomeController> logger, ErrorLoggerService errorLogger, LearningProjectContext context)
        {
            _logger = logger;
            _errorLogger = errorLogger;
            _context = context;
        }


        public async Task<ActionResult> About()
        {
            IQueryable<EnrollmentDateGroup> data =
                from student in _context.Students
                group student by student.EnrollmentDate into dateGroup
                select new EnrollmentDateGroup()
                {
                    EnrollmentDate = dateGroup.Key,
                    StudentCount = dateGroup.Count()
                };
            return View(await data.AsNoTracking().ToListAsync());
        }

        [Authorize(Roles = "HomeIndex")]
        public IActionResult Index()
        {
            /*var User = HttpContext.User.Identities.FirstOrDefault();

            var UserName = User.Name;

            var x = User.Claims.ToList();*/

            try
            {
                // cod normal
                int x = 0;
                int y = 5 / x; // testare eroare
            }
            catch (Exception ex)
            {
                _errorLogger.LogErrorAsync(ex);
            }

            return View();
        }

        [Authorize(Roles="HomePrivacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "HomeError")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
