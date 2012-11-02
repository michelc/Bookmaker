using System.Data;
using System.Linq;
using System.Web.Mvc;
using AutoMapper.QueryableExtensions;
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
            // Détermine si la màj de la brochure est possible
            ViewBag.IsUpdatable = db.BookletIsUpdatable(Root_ID);
            this.RouteData.Values.Add("is_updatable", ViewBag.IsUpdatable);

            var travels = db
                .Travels
                .Where(travel => travel.Booklet_ID == Root_ID)
                .OrderBy(travel => travel.Position)
                .Project().To<TravelIndex>()
                .ToList();

            return View(travels);
        }

        //
        // POST: /Travels/Sort?from=5&to=10

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Sort(int Root_ID, int from, int to)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

            var success = db.SortPositions("Travels", "Booklet_ID", Root_ID, from, to);

            if (!success) Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            return Json(success);
        }

        //
        // GET: /Travels/Details/5

        public ViewResult Details(int Root_ID, int id)
        {
            // Détermine si la màj de la brochure est possible
            ViewBag.IsUpdatable = db.BookletIsUpdatable(Root_ID);
            this.RouteData.Values.Add("is_updatable", ViewBag.IsUpdatable);

            var travel = db.Travels.Find(id);
            travel.Prices = travel.Prices.OrderBy(p => p.Title).ToList();
            travel.Sections = travel.Sections.OrderBy(s => s.Position).ToList();

            return View(travel);
        }

        //
        // GET: /Travels/Next/5

        public RedirectResult Next(int id)
        {
            var travel = db.Travels.Find(id);

            var travels = (from t in db.Travels
                         where (t.Booklet_ID == travel.Booklet_ID)
                         orderby t.Position ascending
                         select new { t.Travel_ID, t.Position });

            var next = (from t in travels
                        where (t.Position > travel.Position)
                        select t.Travel_ID).FirstOrDefault();

            if (next == 0)
            {
                next = (from t in travels
                        select t.Travel_ID).FirstOrDefault();
            }

            return Redirect(Url.Action("Details", new { id = next }));
        }

        //
        // GET: /Travels/Previous/5

        public RedirectResult Previous(int id)
        {
            var travel = db.Travels.Find(id);

            var travels = (from t in db.Travels
                           where (t.Booklet_ID == travel.Booklet_ID)
                           orderby t.Position descending
                           select new { t.Travel_ID, t.Position });

            var prev = (from t in travels
                        where (t.Position < travel.Position)
                        select t.Travel_ID).FirstOrDefault();

            if (prev == 0)
            {
                prev = (from t in travels
                        select t.Travel_ID).FirstOrDefault();
            }

            return Redirect(Url.Action("Details", new { id = prev }));
        }

        //
        // GET: /Travels/Create

        public ActionResult Create(int Root_ID)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

            var travel = new Travel();

            travel.Booklet_ID = Root_ID;

            return View(travel);
        }

        //
        // POST: /Travels/Create

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(int Root_ID, Travel travel)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

            if (ModelState.IsValid)
            {
                // Initialise la position à la prochaine disponible
                travel.Position = db.Travels.Where(t => t.Booklet_ID == travel.Booklet_ID).Count() + 1;

                // Reformate et contrôle le contenu saisi
                travel.Title = InputHelper.ContentFormat(travel.Title);
                travel.Subtitle = InputHelper.ContentFormat(travel.Subtitle);

                // Enregistre le nouveau voyage
                db.Travels.Add(travel);
                db.SaveChanges();

                this.Flash(string.Format("Le voyage {0} a été créé", travel.Title));
                return RedirectToAction("Details", new { id = travel.Travel_ID });
            }

            return View(travel);
        }

        //
        // GET: /Travels/Edit/5

        public ActionResult Edit(int Root_ID, int id)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

            var travel = db.Travels.Find(id);

            return View(travel);
        }

        //
        // POST: /Travels/Edit/5

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(int Root_ID, Travel travel)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

            if (ModelState.IsValid)
            {
                // Reformate et contrôle le contenu saisi
                travel.Title = InputHelper.ContentFormat(travel.Title);
                travel.Subtitle = InputHelper.ContentFormat(travel.Subtitle);

                // Enregistre les modifications
                db.Entry(travel).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash(string.Format("Le voyage {0} a été modifié", travel.Title));
                return RedirectToAction("Details", new { id = travel.Travel_ID });
            }

            return View(travel);
        }

        //
        // GET: /Travels/Delete/5

        public ActionResult Delete(int Root_ID, int id)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

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
        public ActionResult DeleteConfirmed(int Root_ID, int id)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

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

        public ViewResult Copy(int Root_ID, int id)
        {
            // Détermine si la màj de la brochure est possible
            ViewBag.IsUpdatable = db.BookletIsUpdatable(Root_ID);
            this.RouteData.Values.Add("is_updatable", ViewBag.IsUpdatable);

            var travel = db.Travels.Find(id);
            var booklets = db.Booklets
                             .Where(b => b.Booklet_ID != travel.Booklet_ID)
                             .Where(b => b.Booklet_ID != 1) // exclue les brochures "clôturées"
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
        public ActionResult Copy(int Root_ID, int id, int Destination_ID)
        {
            // Vérifie que la màj de la brochure cible est possible
            if (!db.BookletIsUpdatable(Destination_ID)) return new HttpStatusCodeResult(403);

            if (!ModelState.IsValid)
            {
                return Copy(Root_ID, id);
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
