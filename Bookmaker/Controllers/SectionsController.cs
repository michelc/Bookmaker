﻿using System.Data;
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

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Sort(int Root_ID, int Parent_ID, int from, int to)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

            var success = db.SortPositions("Sections", "Travel_ID", Parent_ID, from, to);

            if (!success) Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            return Json(success);
        }

        //
        // GET: /Sections/Create?Parent_ID=5

        public ActionResult Create(int Root_ID, int Parent_ID, int Section_Type = 0)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

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

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(int Root_ID, Section section)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

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
                if (section.SectionType == SectionType.Menu)
                {
                    // A la création, gère un copier / coller depuis ancienne brochure
                    // où le tiret long sert de séparateur entre les plats
                    section.Content = section.Content.Replace(" – ", "\r\n");
                }

                // Enregistre la nouvelle partie
                db.Sections.Add(section);
                db.SaveChanges();

                this.Flash(string.Format("La partie n° {0} a été créée", section.Position));
                return RedirectToAction("Details", "Travels", new { id = section.Travel_ID });
            }

            section.Travel = db.Travels.Find(section.Travel_ID);
            return View(section);
        }

        //
        // GET: /Sections/Edit/5

        public ActionResult Edit(int Root_ID, int id)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

            var section = db.Sections.Find(id);
            section.Content = section.Content.Replace("« ", "\"").Replace(" »", "\"");
            section.Travel = db.Travels.Find(section.Travel_ID);

            return View(section);
        }

        //
        // POST: /Sections/Edit/5

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(int Root_ID, Section section)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

            if (ModelState.IsValid)
            {
                // Reformate et contrôle le contenu saisi
                section.Content = InputHelper.ContentFormat(section.Content);
                if (section.SectionType == SectionType.Titre)
                {
                    section.Content = InputHelper.TitleFormat(section.Content);
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

        public ActionResult Delete(int Root_ID, int id)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

            var section = db.Sections.Find(id);
            section.Travel = db.Travels.Find(section.Travel_ID);

            return View(section);
        }

        //
        // POST: /Sections/Delete/5

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int Root_ID, int id)
        {
            // Vérifie que la màj de la brochure est possible
            if (!db.BookletIsUpdatable(Root_ID)) return new HttpStatusCodeResult(403);

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