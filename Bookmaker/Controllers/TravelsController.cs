using System.Data;
using System.Linq;
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
                    Travel_ID = travel.Travel_ID,
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
                db.ExecuteSql(sql);

                if (from < to)
                {
                    // Ramène d'un rang tous les élements entre le départ et l'arrivée
                    sql = string.Format("UPDATE Travels SET Position = Position - 1 WHERE Position BETWEEN {0} AND {1}", from, to);
                    db.ExecuteSql(sql);
                }
                else
                {
                    // Repousse d'un rang tous les élements entre l'arrivée et le départ
                    sql = string.Format("UPDATE Travels SET Position = Position + 1 WHERE Position BETWEEN {0} AND {1}", to, from);
                    db.ExecuteSql(sql);
                }

                // Déplace l'élément mis de coté à la position d'arrivée
                sql = string.Format("UPDATE Travels SET Position = {0} WHERE Position = 0", to);
                db.ExecuteSql(sql);

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
            travel.Prices = travel.Prices.OrderBy(p => p.Title).ThenBy(p => p.Price_ID).ToList();
            travel.Sections = travel.Sections.OrderBy(s => s.Position).ToList();

            return View(travel);
        }

        //
        // GET: /Travels/Create

        public ViewResult Create()
        {
            var travel = new Travel();

            travel.Booklet_ID = 1; // (pour l'instant)

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
                // Enregistre le nouveau voyage
                db.Travels.Add(travel);
                db.SaveChanges();

                this.Flash(string.Format("Le voyage {0} a été créé", travel.Title));
                return RedirectToAction("Details", new { id = travel.Travel_ID });
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
                return RedirectToAction("Details", new { id = travel.Travel_ID });
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
            db.ExecuteSql(sql);

            this.Flash(string.Format("Le voyage {0} a été supprimé", travel.Title));
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
