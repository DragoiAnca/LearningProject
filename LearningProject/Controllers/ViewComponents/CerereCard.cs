using LearningProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningProject.Controllers.ViewComponents
{
    public class CerereCard : ViewComponent
    {
        private readonly LearningProjectContext _db;

        public CerereCard(LearningProjectContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cerere = await _db.Cereri
                      .Include(c => c.CreatedByUser)
                      .Include(c => c.DeletedBy)
                      .ToListAsync();

            return View(cerere);
        }
    }
}