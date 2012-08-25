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

        public ViewResult Index(int Root_ID)
        {
            var travels = db
                .Travels
                .Where(travel => travel.Booklet_ID == Root_ID)
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

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Sort(int Root_ID, int from, int to)
        {
            var success = db.SortPositions("Travels", "Booklet_ID", Root_ID, from, to);

            if (!success) Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            return Json(success);
        }

        //
        // GET: /Travels/Details/5

        public ViewResult Details(int id)
        {
            var travel = db.Travels.Find(id);
            travel.Prices = travel.Prices.OrderBy(p => p.Title).ToList();
            travel.Sections = travel.Sections.OrderBy(s => s.Position).ToList();

            return View(travel);
        }

        //
        // GET: /Travels/Copy/5

        public ActionResult Copy(int id)
        {
            // Retrouve le voyage à copier
            var travel = db.Travels.Find(id);
            travel.Prices = travel.Prices.OrderBy(p => p.Title).ToList();
            travel.Sections = travel.Sections.OrderBy(s => s.Position).ToList();

            // Rattache ce voyage à une brochure fictive
            var src_booklet = new Booklet
            {
                Travels = new System.Collections.Generic.List<Travel>()
            };
            src_booklet.Travels.Add(travel);

            // Sérialise cette brochure fictive (en passant par une liste de brochure)
            var src_booklets = new System.Collections.Generic.List<Booklet>();
            src_booklets.Add(src_booklet);
            var json = ImportExport.JsonExport(src_booklets);

            // Désérialisation (bidouille pour faire une copie complète)
            var dest_booklets = ImportExport.JsonImport(json);

            // Retrouve la 1° (et seule) brochure (de la liste de brochure obtenue)
            var dest_booklet = dest_booklets.First();

            // Retrouve le 1° (et seul) voyage de cette brochure
            var dest_travel = dest_booklet.Travels.First();

            // Retrouve la brochure de destination (id codé en dur)
            var destination = db.Booklets.Find(2);

            // Lui ajoute le voyage copié
            destination.Travels.Add(dest_travel);

            // Sauvegarde !
            db.SaveChanges();

            this.Flash(string.Format("Le voyage {0} a été copié", dest_travel.Title));
            return RedirectToAction("Details", new { root_id = 2, id = dest_travel.Travel_ID });
        }

        //
        // GET: /Travels/Create

        public ViewResult Create(int Root_ID)
        {
            var travel = new Travel();

            travel.Booklet_ID = Root_ID;

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
                travel.Position = db.Travels.Where(t => t.Booklet_ID == travel.Booklet_ID).Count() + 1;

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

            if (travel.Prices.Count + travel.Sections.Count > 0)
            {
                ViewBag.Warning = "Attention, ce voyage contient des tarifs et des parties qui seront perdues !";
            }

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
            db.RefillPositions("Travels", "Booklet_ID", travel.Booklet_ID, travel.Position);

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
