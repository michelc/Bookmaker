using System.Linq;
using System.Web.Mvc;
using Bookmaker.Models;

namespace Bookmaker.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int root_id = 1)
        {
            if (root_id == 0)
            {
                try
                {
                    var db = new BookmakerContext();
                    root_id = (from b in db.Booklets
                               orderby b.Booklet_ID descending
                               select b.Booklet_ID).FirstOrDefault();
                }
                catch
                {
                    root_id = 1;
                }

                if (root_id == 0) root_id = 1;
                return RedirectToAction("Index", new { root_id = root_id });
            }

            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            // return View();

            throw new System.Exception("Quelquefois une erreur survient !");
        }
    }
}
