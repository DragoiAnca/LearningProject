using Microsoft.AspNetCore.Mvc;

namespace LearningProject.Controllers.ViewComponents
{
    public class MyComponent : ViewComponent
    {
        // This method is called when the component is invoked
        public IViewComponentResult Invoke(string message)
        {
            // Pass data to the view
            return View("Default", message);
        }
    }
}