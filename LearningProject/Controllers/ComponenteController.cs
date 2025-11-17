using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LearningProject.Controllers
{
    public class ComponenteController : Controller
    {
        // GET: Componente
        public ActionResult Index()
        {
            return View();
        }

        // GET: Componente/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Componente/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Componente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Componente/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Componente/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Componente/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Componente/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
