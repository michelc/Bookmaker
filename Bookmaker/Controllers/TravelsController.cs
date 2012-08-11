using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Bookmaker.Helpers;
using Bookmaker.Models;

namespace Bookmaker.Controllers
{
    public class TravelsController : Controller
    {
        private BookmakerContext db = new BookmakerContext();

        //
        // GET: /Travels/

        public ViewResult Index()
        {
            var travels = db
                .Travels
                .OrderBy(travel => travel.Position)
                .ThenBy(travel => travel.Title)
                .Select(travel => new TravelIndex
                {
                    TravelID = travel.TravelID,
                    Position = travel.Position,
                    Title = travel.Title,
                    TravelType = travel.TravelType,
                    PricesCount = travel.Prices.Count(),
                    SectionsCount = travel.Sections.Count()
                }).ToList();

            return View(travels);
        }

        //
        // POST: /Travels/Sort?from=5&to=10

        [HttpPost]
        public JsonResult Sort(int from, int to)
        {
            var result = string.Format("TODO : déplacer voyage de {0}° position vers la {1}°", from + 1, to + 1);

            return Json(result);
        }

        //
        // GET: /Travels/Details/5

        public ViewResult Details(int id)
        {
            var travel = db.Travels.Find(id);
            travel.Prices = travel.Prices.OrderBy(p => p.Year).ThenBy(p => p.Title).ThenBy(p => p.PriceID).ToList();
            travel.Sections = travel.Sections.OrderBy(s => s.Position).ThenBy(s => s.SectionID).ToList();

            return View(travel);
        }

        //
        // GET: /Travels/Create

        public ViewResult Create()
        {
            var travel = new Travel();

            // Initialise la position à la prochaine disponible
            travel.Position = (from t in db.Travels
                               orderby t.Position descending
                               select t.Position).FirstOrDefault() + 1;

            ViewBag.TravelType = db.Enums<TravelType>();
            return View(travel);
        }

        //
        // POST: /Travels/Create

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Travel travel)
        {
            if (ModelState.IsValid)
            {
                db.Travels.Add(travel);
                db.SaveChanges();

                this.Flash(string.Format("Le voyage {0} a été créé", travel.Title));
                return RedirectToAction("Details", new { id = travel.TravelID });
            }

            ViewBag.TravelType = db.Enums<TravelType>();
            return View(travel);
        }

        //
        // GET: /Travels/Edit/5

        public ViewResult Edit(int id)
        {
            var travel = db.Travels.Find(id);

            ViewBag.TravelType = db.Enums<TravelType>();
            return View(travel);
        }

        //
        // POST: /Travels/Edit/5

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Travel travel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(travel).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash(string.Format("Le voyage {0} a été modifié", travel.Title));
                return RedirectToAction("Details", new { id = travel.TravelID });
            }

            ViewBag.TravelType = db.Enums<TravelType>();
            return View(travel);
        }

        //
        // GET: /Travels/Delete/5

        public ViewResult Delete(int id)
        {
            var travel = db.Travels.Find(id);

            return View(travel);
        }

        //
        // POST: /Travels/Delete/5

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var travel = db.Travels.Find(id);
            db.Travels.Remove(travel);
            db.SaveChanges();

            this.Flash(string.Format("Le voyage {0} a été supprimé", travel.Title));
            return RedirectToAction("Index");
        }

        //
        // GET: /Travels/JsonExport

        public ContentResult JsonExport()
        {
            // Retrouve tous les voyages
            var travels = db
                .Travels
                .Include(t => t.Sections)
                .Include(t => t.Prices)
                .OrderBy(t => t.Position)
                .ThenBy(t => t.Title);

            // Transforme les données au format Json
            var json = ImportExport.JsonExport(travels);

            // Sauvegarde les données au format Json
            // (quand on est sur http://localhost/)
            if (Request.Url.IsLoopback)
            {
                var file = Server.MapPath("~/App_Data/json_db.txt");
                System.IO.File.WriteAllText(file, json);
            }

            // Renvoie les données au format Json
            return Content(json, "application/json", Encoding.Default);
        }

        //
        // GET: /Travels/JsonImport

        public ActionResult JsonImport()
        {
            // Vide les tables actuelles
            db.TruncateTable("Prices", "PriceID");
            db.TruncateTable("Sections", "SectionID");
            db.TruncateTable("Travels", "TravelID");

            // Charge les données à importer
            var file = Server.MapPath("~/App_Data/json_db.txt");
            var json = System.IO.File.ReadAllText(file);

            // Importe les données
            var travels = ImportExport.JsonImport(json);

            // Insère les données importées dans la base de données
            travels.ForEach(t => db.Travels.Add(t));
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
