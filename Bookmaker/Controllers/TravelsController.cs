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

        [BookletUpdatable(Continue = true)]
        public ViewResult Index(int root_id)
        {
            var travels = db
                .Travels
                .Where(travel => travel.Booklet_ID == root_id)
                .OrderBy(travel => travel.Position)
                .MapTo<TravelIndex>()
                .ToList();

            return View(travels);
        }

        //
        // GET: /Travels/Search?q=xxx

        public ViewResult Search(string q)
        {
            var search_in_travels = db
                .Travels
                .Where(travel => travel.Title.Contains(q))
                .MapTo<TravelSearch>()
                .ToList();

            var search_in_sections = db
                .Sections
                .Where(section => section.Content.Contains(q))
                .MapTo<TravelSearch>()
                .Distinct()
                .ToList();

            foreach (var found in search_in_travels)
            {
                search_in_sections.RemoveAll(result => result.Travel_ID == found.Travel_ID);
            }

            var results = search_in_travels
                .Union(search_in_sections)
                .OrderByDescending(result => result.BookletYear)
                .ThenByDescending(result => result.Booklet_ID)
                .ThenBy(result => result.Position)
                .ToList();

            ViewBag.q = q;
            return View(results);
        }

        //
        // POST: /Travels/Sort?from=5&to=10

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable()]
        public ActionResult Sort(int root_id, int from, int to)
        {
            var success = db.SortPositions("Travels", "Booklet_ID", root_id, from, to);

            if (!success) Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            return Json(success);
        }

        //
        // GET: /Travels/Details/5

        [BookletUpdatable(Continue = true)]
        public ViewResult Details(int id)
        {
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

        [BookletUpdatable()]
        public ActionResult Create(int root_id)
        {
            var travel = new Travel();

            travel.Booklet_ID = root_id;

            return View(travel);
        }

        //
        // POST: /Travels/Create

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable()]
        public ActionResult Create(Travel travel)
        {
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

                this.Flash("Le voyage {0} a été créé", travel.Title);
                return RedirectToAction("Details", new { id = travel.Travel_ID });
            }

            return View(travel);
        }

        //
        // GET: /Travels/Edit/5

        [BookletUpdatable()]
        public ActionResult Edit(int id)
        {
            var travel = db.Travels.Find(id);

            return View(travel);
        }

        //
        // POST: /Travels/Edit/5

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable()]
        public ActionResult Edit(Travel travel)
        {
            if (ModelState.IsValid)
            {
                // Reformate et contrôle le contenu saisi
                travel.Title = InputHelper.ContentFormat(travel.Title);
                travel.Subtitle = InputHelper.ContentFormat(travel.Subtitle);

                // Enregistre les modifications
                db.Entry(travel).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash("Le voyage {0} a été modifié", travel.Title);
                return RedirectToAction("Details", new { id = travel.Travel_ID });
            }

            return View(travel);
        }

        //
        // GET: /Travels/Delete/5

        [BookletUpdatable()]
        public ActionResult Delete(int id)
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

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable(), ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            // Supprime le voyage
            var travel = db.Travels.Find(id);
            db.Travels.Remove(travel);
            db.SaveChanges();

            // Réordonne les voyages
            db.RefillPositions("Travels", "Booklet_ID", travel.Booklet_ID, travel.Position);

            this.Flash("Le voyage {0} a été supprimé", travel.Title);
            return RedirectToAction("Index");
        }

        //
        // GET: /Travels/Copy/5

        [BookletUpdatable(Continue = true)]
        public ViewResult Copy(int id)
        {
            var travel = db.Travels.Find(id);
            var booklets = db.Booklets
                             .Where(b => b.Booklet_ID != travel.Booklet_ID)
                             .Where(b => b.IsReadOnly == false)
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

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable(Name="Destination_ID")]
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

            // Retrouve la brochure d'origine
            var origine = db.Booklets.Find(travel.Booklet_ID);

            // Retrouve la brochure de destination
            var destination = db.Booklets.Find(Destination_ID);

            // Renomme éventuellement les images
            copy.Sections.ToList().ForEach(s =>
            {
                if (s.SectionType == SectionType.Image)
                {
                    if (s.Content.Contains(origine.Year))
                    {
                        s.Content = s.Content.Replace(origine.Year, destination.Year);
                    }
                }
            });

            // Lui ajoute le voyage copié en dernière position
            copy.Position = db.Travels.Where(t => t.Booklet_ID == Destination_ID).Count() + 1;
            destination.Travels.Add(copy);

            // Sauvegarde !
            db.SaveChanges();

            this.Flash("Le voyage {0} a été copié", copy.Title);
            return RedirectToAction("Details", new { root_id = Destination_ID, id = copy.Travel_ID });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}