using Microsoft.AspNetCore.Mvc;

namespace LearningProject.Controllers.ViewComponents
{
    public class CardTwo : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }

    }
}
