using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Models.ViewModels;
using LearningProject.Services.Impl;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningProject.Controllers
{

    [Route("api/cereri")]
    [ApiController]
    public class CereriControllerApi : Controller
    {

        private readonly LearningProjectContext _context;
        private readonly ICereri _cereriService;

        public CereriControllerApi(LearningProjectContext context, ICereri cereriService)
        {
            _context = context;
            _cereriService = cereriService;
        }
        //creaza constructor pentru a incarca servicul
        //pune annnotati
        //corecteaza comentarile

        [HttpPost]
        public async Task<ActionResult<Cereri>> cereri(CreateNewCerereModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentUserName = User.FindFirstValue(ClaimTypes.Name)
                .Replace("MMRMAKITA\\", "");

            var cerere = await _cereriService.CreateCerereAsync(model, currentUserName);

            return Ok(cerere);

        }
    }
}
