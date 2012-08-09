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

            return View(section);
        }

        //
        // GET: /Sections/Create?int ParentID=5

        public ViewResult Create(int ParentID)
        {
            var section = new Section
            {
                TravelID = ParentID,
                Travel = db.Travels.Find(ParentID)
            };

            // Initialise la position à la prochaine disponible
            section.Position = (from s in db.Sections
                                where s.TravelID == ParentID
                                orderby s.Position descending
                                select s.Position).FirstOrDefault() + 1;

            ViewBag.SectionType = db.Enums<SectionType>();
            return View(section);
        }

        //
        // POST: /Sections/Create

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Section section)
        {
            if (ModelState.IsValid)
            {
                section.Content = SectionHelper.ContentFormat(section.Content);
                if (section.TypeSection == SectionType.Titre)
                {
                    section.Content = SectionHelper.TitleFormat(section.Content);
                }

                db.Sections.Add(section);
                db.SaveChanges();

                this.Flash(string.Format("La partie {0} a été créé", section.Position));
                return RedirectToAction("Details", "Travels", new { id = section.TravelID });
            }

            section.Travel = db.Travels.Find(section.TravelID);
            ViewBag.SectionType = db.Enums<SectionType>();
            return View(section);
        }

        //
        // GET: /Sections/Edit/5

        public ActionResult Edit(int id)
        {
            var section = db.Sections.Find(id);
            section.Travel = db.Travels.Find(section.TravelID);

            ViewBag.SectionType = db.Enums<SectionType>();
            return View(section);
        }

        //
        // POST: /Sections/Edit/5

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Section section)
        {
            if (ModelState.IsValid)
            {
                section.Content = SectionHelper.ContentFormat(section.Content);
                if (section.TypeSection == SectionType.Titre)
                {
                    section.Content = SectionHelper.TitleFormat(section.Content);
                }

                db.Entry(section).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash(string.Format("La partie {0} a été modifié", section.Position));
                return RedirectToAction("Details", "Travels", new { id = section.TravelID });
            }

            section.Travel = db.Travels.Find(section.TravelID);
            ViewBag.SectionType = db.Enums<SectionType>();
            return View(section);
        }

        //
        // GET: /Sections/Delete/5

        public ActionResult Delete(int id)
        {
            var section = db.Sections.Find(id);

            return View(section);
        }

        //
        // POST: /Sections/Delete/5

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var section = db.Sections.Find(id);
            db.Sections.Remove(section);
            db.SaveChanges();

            this.Flash(string.Format("La partie {0} a été supprimé", section.Position));
            return RedirectToAction("Details", "Travels", new { id = section.TravelID });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}