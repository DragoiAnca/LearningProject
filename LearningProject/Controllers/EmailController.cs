using LearningProject.Models;
using LearningProject.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LearningProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly SendEmailService _emailService;

        public EmailController(SendEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] Email request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _emailService.SendEmailAsync(request);

            return Ok(result);
        }
    }
}
