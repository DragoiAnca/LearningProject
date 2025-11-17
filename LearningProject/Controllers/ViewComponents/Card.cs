using Microsoft.AspNetCore.Mvc;

namespace LearningProject.Controllers.ViewComponents 
{ 
    public class Card : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
