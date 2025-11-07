using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningProject.Data;
using LearningProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace LearningProject.Controllers
{
    public class ErrorLogsController : Controller
    {
        private readonly LearningProjectContext _context;

        public ErrorLogsController(LearningProjectContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "ErrorLogsIndex")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.ErrorLogs.ToListAsync());
        }

        [Authorize(Roles = "ErrorLogsDetails")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var errorLog = await _context.ErrorLogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (errorLog == null)
            {
                return NotFound();
            }

            return View(errorLog);
        }

        [Authorize(Roles = "ErrorLogsCreate")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "ErrorLogsCreate")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ErrorMessage,StackTrace,DateOccurred")] ErrorLog errorLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(errorLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(errorLog);
        }

        [Authorize(Roles = "ErrorLogsEdit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var errorLog = await _context.ErrorLogs.FindAsync(id);
            if (errorLog == null)
            {
                return NotFound();
            }
            return View(errorLog);
        }

        [Authorize(Roles = "ErrorLogsEdit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ErrorMessage,StackTrace,DateOccurred")] ErrorLog errorLog)
        {
            if (id != errorLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(errorLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ErrorLogExists(errorLog.Id))
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
            return View(errorLog);
        }

        [Authorize(Roles = "ErrorLogsDelete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var errorLog = await _context.ErrorLogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (errorLog == null)
            {
                return NotFound();
            }

            return View(errorLog);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var errorLog = await _context.ErrorLogs.FindAsync(id);
            if (errorLog != null)
            {
                _context.ErrorLogs.Remove(errorLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ErrorLogExists(int id)
        {
            return _context.ErrorLogs.Any(e => e.Id == id);
        }
    }
}
