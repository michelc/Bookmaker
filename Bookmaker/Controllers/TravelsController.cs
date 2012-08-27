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

        //
        // GET: /Travels/Copy/5

        public ViewResult Copy(int id)
        {
            var travel = db.Travels.Find(id);
            var booklets = db.Booklets
                             .Where(b => b.Booklet_ID != travel.Booklet_ID)
                             .OrderByDescending(b => b.Year)
                             .ThenBy(b => b.Title);

            var copy = new TravelCopy
            {
                Travel = travel,
                Destinations = new SelectList(booklets, "Booklet_ID", "Title")
            };

            return View(copy);
        }

        //
        // POST: /Travels/Copy/5

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Copy(int id, int Destination_ID)
        {
            if (!ModelState.IsValid)
            {
                return Copy(id);
            }

            // Retrouve le voyage à copier
            var travel = db.Travels.Find(id);
            travel.Prices = travel.Prices.OrderBy(p => p.Title).ToList();
            travel.Sections = travel.Sections.OrderBy(s => s.Position).ToList();

            // Sérialisation / désérialisation
            var temp = AutoMapper.Mapper.Map<Travel, JsonTravel>(travel);
            var copy = AutoMapper.Mapper.Map<JsonTravel, Travel>(temp);

            // Positionne chaque partie du voyage
            int position = 0;
            copy.Sections.ToList().ForEach(s => s.Position = ++position);

            // Retrouve la brochure de destination (id codé en dur)
            var destination = db.Booklets.Find(Destination_ID);

            // Lui ajoute le voyage copié en dernière position
            copy.Position = db.Travels.Where(t => t.Booklet_ID == Destination_ID).Count() + 1;
            destination.Travels.Add(copy);

            // Sauvegarde !
            db.SaveChanges();

            this.Flash(string.Format("Le voyage {0} a été copié", copy.Title));
            return RedirectToAction("Details", new { root_id = Destination_ID, id = copy.Travel_ID });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
