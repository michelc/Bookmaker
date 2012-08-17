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

        [HttpPost]
        public JsonResult Sort(int Parent_ID, int from, int to)
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
                sql = string.Format("UPDATE Sections SET Position = 0 WHERE Travel_ID = {0} AND Position = {1}", Parent_ID, from);
                db.ExecuteSql(sql);

                if (from < to)
                {
                    // Ramène d'un rang tous les élements entre le départ et l'arrivée
                    sql = string.Format("UPDATE Sections SET Position = Position - 1 WHERE Travel_ID = {0} AND Position BETWEEN {1} AND {2}", Parent_ID, from, to);
                    db.ExecuteSql(sql);
                }
                else
                {
                    // Repousse d'un rang tous les élements entre l'arrivée et le départ
                    sql = string.Format("UPDATE Sections SET Position = Position + 1 WHERE Travel_ID = {0} AND Position BETWEEN {1} AND {2}", Parent_ID, to, from);
                    db.ExecuteSql(sql);
                }

                // Déplace l'élément mis de coté à la position d'arrivée
                sql = string.Format("UPDATE Sections SET Position = {0} WHERE Travel_ID = {1} AND Position = 0", to, Parent_ID);
                db.ExecuteSql(sql);

                // Tout va bien
                result = string.Empty;
            }
            catch { }

            return Json(result);
        }

        //
        // GET: /Sections/Create?Parent_ID=5

        public ViewResult Create(int Parent_ID, int SectionType = 0)
        {
            var section = new Section
            {
                Travel_ID = Parent_ID,
                Travel = db.Travels.Find(Parent_ID)
            };
            if (SectionType != 0)
            {
                section.SectionType = SectionType;
            }

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
                // Initialise la position à la prochaine disponible
                section.Position = db.Sections.Where(s => s.Travel_ID == section.Travel_ID).Count() + 1;

                // Reformate et contrôle le contenu saisi
                section.Content = InputHelper.ContentFormat(section.Content);
                if (section.TypeSection == SectionType.Titre)
                {
                    section.Content = InputHelper.TitleFormat(section.Content);
                }
                if (section.TypeSection == SectionType.Menu)
                {
                    // A la création, gère un copier / coller depuis ancienne brochure
                    // où le tiret long sert de séparateur entre les plats
                    section.Content = section.Content.Replace(" – ", "\r\n"); ;
                }

                // Enregistre la nouvelle partie
                db.Sections.Add(section);
                db.SaveChanges();

                this.Flash(string.Format("La partie {0} a été créé", section.Position));
                return RedirectToAction("Details", "Travels", new { id = section.Travel_ID });
            }

            section.Travel = db.Travels.Find(section.Travel_ID);
            ViewBag.SectionType = db.Enums<SectionType>();
            return View(section);
        }

        //
        // GET: /Sections/Edit/5

        public ActionResult Edit(int id)
        {
            var section = db.Sections.Find(id);
            section.Travel = db.Travels.Find(section.Travel_ID);

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
                section.Content = InputHelper.ContentFormat(section.Content);
                if (section.TypeSection == SectionType.Titre)
                {
                    section.Content = InputHelper.TitleFormat(section.Content);
                }

                db.Entry(section).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash(string.Format("La partie {0} a été modifié", section.Position));
                return RedirectToAction("Details", "Travels", new { id = section.Travel_ID });
            }

            section.Travel = db.Travels.Find(section.Travel_ID);
            ViewBag.SectionType = db.Enums<SectionType>();
            return View(section);
        }

        //
        // GET: /Sections/Delete/5

        public ActionResult Delete(int id)
        {
            var section = db.Sections.Find(id);
            section.Travel = db.Travels.Find(section.Travel_ID);

            return View(section);
        }

        //
        // POST: /Sections/Delete/5

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            // Supprime la partie du voyage
            var section = db.Sections.Find(id);
            db.Sections.Remove(section);
            db.SaveChanges();

            // Réordonne les parties du voyages
            var sql = string.Format("UPDATE Sections SET Position = Position - 1 WHERE Travel_ID = {0} AND Position > {1}", section.Travel_ID, section.Position);
            db.ExecuteSql(sql);

            this.Flash(string.Format("La partie {0} a été supprimé", section.Position));
            return RedirectToAction("Details", "Travels", new { id = section.Travel_ID });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}