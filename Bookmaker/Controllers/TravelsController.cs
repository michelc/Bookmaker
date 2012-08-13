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
            var result = "Le nouvel ordre de tri s'est mal enregistré";

            try
            {
                // Les positions démarrent à 1
                // (alors que les index jQuery commencent à 0)
                from++;
                to++;

                var sql = string.Empty;

                // Met de côté l'élément à la position de départ
                sql = string.Format("UPDATE Travels SET Position = 0 WHERE Position = {0}", from);
                db.Database.ExecuteSqlCommand(sql);

                if (from < to)
                {
                    // Ramène d'un rang tous les élements entre le départ et l'arrivée
                    sql = string.Format("UPDATE Travels SET Position = Position - 1 WHERE Position BETWEEN {0} AND {1}", from, to);
                    db.Database.ExecuteSqlCommand(sql);
                }
                else
                {
                    // Repousse d'un rang tous les élements entre l'arrivée et le départ
                    sql = string.Format("UPDATE Travels SET Position = Position + 1 WHERE Position BETWEEN {0} AND {1}", to, from);
                    db.Database.ExecuteSqlCommand(sql);
                }

                // Déplace l'élément mis de coté à la position d'arrivée
                sql = string.Format("UPDATE Travels SET Position = {0} WHERE Position = 0", to);
                db.Database.ExecuteSqlCommand(sql);

                // Tout va bien
                result = string.Empty;
            } catch {}

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
                // Initialise la position à la prochaine disponible
                travel.Position = db.Travels.Count() + 1;
                // enregistre le nouveau voyage
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
            // Supprime le voyage
            var travel = db.Travels.Find(id);
            db.Travels.Remove(travel);
            db.SaveChanges();

            // Réordonne les voyages
            var sql = string.Format("UPDATE Travels SET Position = Position - 1 WHERE Position > {0}", travel.Position);
            db.Database.ExecuteSqlCommand(sql);

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
                .OrderBy(t => t.Position);

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
