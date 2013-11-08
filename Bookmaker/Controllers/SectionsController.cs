using System.Data;
using System.Linq;
using System.Web.Mvc;
using Bookmaker.Helpers;
using Bookmaker.Models;

namespace Bookmaker.Controllers
{
    public class SectionsController : Controller
    {
        private BookmakerContext db = new BookmakerContext();

        //
        // GET: /Sections/Details/5

        public ViewResult Details(int id)
        {
            var section = db.Sections.Find(id);
            section.Travel = db.Travels.Find(section.Travel_ID);

            return View(section);
        }

        //
        // POST: /Sections/Sort?Parent_ID=1&from=5&to=10

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable()]
        public ActionResult Sort(int Parent_ID, int from, int to)
        {
            var success = db.SortPositions("Sections", "Travel_ID", Parent_ID, from, to);

            if (!success) Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            return Json(success);
        }

        //
        // GET: /Travels/Import?Parent_ID=5

        [BookletUpdatable()]
        public ActionResult Import(int Parent_ID)
        {
            var travel = db.Travels.Find(Parent_ID);

            var import = new SectionImport { Travel = travel };
            return View(import);
        }

        //
        // POST: /Travels/Import?Parent_ID=5

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable()]
        public ActionResult Import(int Parent_ID, string rawcontent)
        {
            var travel = db.Travels.Find(Parent_ID);

            if (ModelState.IsValid)
            {
                // Reformate et contrôle le contenu saisi
                travel.Sections = InputHelper.ContentImport(travel.TravelType, rawcontent);

                // Enregistre les modifications
                db.Entry(travel).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash(string.Format("Le voyage {0} a été initialisé", travel.Title));
                return RedirectToAction("Details", "Travels", new { id = travel.Travel_ID });
            }

            var import = new SectionImport { Travel = travel, RawContent = rawcontent };
            return View(import);
        }

        //
        // GET: /Sections/Create?Parent_ID=5

        [BookletUpdatable()]
        public ActionResult Create(int Parent_ID, int Section_Type = 0)
        {
            var section = new Section
            {
                Travel_ID = Parent_ID,
                Travel = db.Travels.Find(Parent_ID)
            };
            if (Section_Type != 0)
            {
                section.SectionType = (SectionType)Section_Type;
                if (section.SectionType == SectionType.Tarif)
                {
                    // Seul le type de section compte pour les tarifs
                    // (mais le contenu est obligatoire)
                    section.Content = "*";
                }
            }

            return View(section);
        }

        //
        // POST: /Sections/Create

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable()]
        public ActionResult Create(Section section)
        {
            if (ModelState.IsValid)
            {
                // Initialise la position à la prochaine disponible
                section.Position = db.Sections.Where(s => s.Travel_ID == section.Travel_ID).Count() + 1;

                // Reformate et contrôle le contenu saisi
                section.Content = InputHelper.ContentFormat(section.Content);
                if (section.SectionType == SectionType.Titre)
                {
                    section.Content = InputHelper.TitleFormat(section.Content);
                }
                else if (section.SectionType == SectionType.Menu)
                {
                    section.Content = InputHelper.MenuFormat(section.Content);
                }

                // Enregistre la nouvelle partie
                db.Sections.Add(section);
                db.SaveChanges();

                // Déplace la 1° image insérée en 1° position
                var position = section.Position;
                if (section.SectionType == SectionType.Image)
                {
                    if (section.Position > 1)
                    {
                        if (db.Sections.Where(s => s.Travel_ID == section.Travel_ID).ToList().Where(s => s.SectionType == SectionType.Image).Count() == 1)
                        {
                            var success = db.SortPositions("Sections", "Travel_ID", section.Travel_ID, section.Position - 1, 0);
                            position = 1;
                        }
                    }
                }

                this.Flash(string.Format("La partie n° {0} a été créée", position));
                return RedirectToAction("Details", "Travels", new { id = section.Travel_ID });
            }

            section.Travel = db.Travels.Find(section.Travel_ID);
            return View(section);
        }

        //
        // GET: /Sections/Edit/5

        [BookletUpdatable()]
        public ActionResult Edit(int id)
        {
            var section = db.Sections.Find(id);
            section.Content = section.Content.Replace("« ", "\"").Replace(" »", "\"");
            section.Travel = db.Travels.Find(section.Travel_ID);

            return View(section);
        }

        //
        // POST: /Sections/Edit/5

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable()]
        public ActionResult Edit(Section section)
        {
            if (ModelState.IsValid)
            {
                // Reformate et contrôle le contenu saisi
                section.Content = InputHelper.ContentFormat(section.Content);
                if (section.SectionType == SectionType.Titre)
                {
                    section.Content = InputHelper.TitleFormat(section.Content);
                }
                else if (section.SectionType == SectionType.Menu)
                {
                    section.Content = InputHelper.MenuFormat(section.Content);
                }

                // Enregistre les modifications
                db.Entry(section).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash(string.Format("La partie n° {0} a été modifiée", section.Position));
                return RedirectToAction("Details", "Travels", new { id = section.Travel_ID });
            }

            section.Travel = db.Travels.Find(section.Travel_ID);
            return View(section);
        }

        //
        // GET: /Sections/Delete/5

        [BookletUpdatable()]
        public ActionResult Delete(int id)
        {
            var section = db.Sections.Find(id);
            section.Travel = db.Travels.Find(section.Travel_ID);

            return View(section);
        }

        //
        // POST: /Sections/Delete/5

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable(), ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            // Supprime la partie du voyage
            var section = db.Sections.Find(id);
            db.Sections.Remove(section);
            db.SaveChanges();

            // Réordonne les parties du voyages
            db.RefillPositions("Sections", "Travel_ID", section.Travel_ID, section.Position);

            this.Flash(string.Format("La partie n° {0} a été supprimée", section.Position));
            return RedirectToAction("Details", "Travels", new { id = section.Travel_ID });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}