using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using AutoMapper.QueryableExtensions;
using Bookmaker.Helpers;
using Bookmaker.Models;

namespace Bookmaker.Controllers
{
    public class BookletsController : Controller
    {
        private BookmakerContext db = new BookmakerContext();

        //
        // GET: /Booklets/

        public ActionResult Index()
        {
            var booklets = db
                .Booklets
                .OrderByDescending(booklet => booklet.Year)
                .ThenBy(booklet => booklet.Title)
                .Project().To<BookletIndex>()
                .ToList();

            return View(booklets);
        }

        //
        // GET: /5/Booklets/Title

        [ChildActionOnly]
        [OutputCache(Duration = 60, VaryByParam = "root_id")]
        public ContentResult Title(int root_id)
        {
            var title = (from b in db.Booklets
                         where b.Booklet_ID == root_id
                         select b.Title).FirstOrDefault();

            return Content(title);
        }

        //
        // GET: /Booklets/Details/5

        public ViewResult Details(int id)
        {
            var booklet = db.Booklets.Find(id);

            return View(booklet);
        }

        //
        // GET: /Booklets/Create

        public ViewResult Create()
        {
            var booklet = new Booklet();

            return View(booklet);
        }

        //
        // POST: /Booklets/Create

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Booklet booklet)
        {
            if (ModelState.IsValid)
            {
                // Enregistre la nouvelle brochure
                db.Booklets.Add(booklet);
                db.SaveChanges();

                this.Flash(string.Format("La brochure {0} a été créée", booklet.Title));
                return RedirectToAction("Details", new { id = booklet.Booklet_ID });
            }

            return View(booklet);
        }

        //
        // GET: /Booklets/Edit/5

        public ViewResult Edit(int id)
        {
            var booklet = db.Booklets.Find(id);

            return View(booklet);
        }

        //
        // POST: /Booklets/Edit/5

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Booklet booklet)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booklet).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash(string.Format("La brochure {0} a été modifiée", booklet.Title));
                return RedirectToAction("Details", new { id = booklet.Booklet_ID });
            }

            return View(booklet);
        }

        //
        // GET: /Booklets/Delete/5

        public ActionResult Delete(int id)
        {
            var booklet = db.Booklets.Find(id);

            if (booklet.Travels.Count > 0)
            {
                this.Flash("!Suppression interdite car la brochure contient des voyages");
                return RedirectToAction("Details", new { id = booklet.Booklet_ID });
            }

            return View(booklet);
        }

        //
        // POST: /Booklets/Delete/5

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            // Supprime la brochure
            var booklet = db.Booklets.Find(id);
            db.Booklets.Remove(booklet);
            db.SaveChanges();

            this.Flash(string.Format("La brochure {0} a été supprimée", booklet.Title));
            return RedirectToAction("Index");
        }

        //
        // GET: /Booklets/Generate/5

        public FileResult Generate(int id)
        {
            // Retrouve tous les voyages de la brochure
            var travels = db
                .Travels
                .Where(t => t.Booklet_ID == id)
                .Include(t => t.Sections)
                .Include(t => t.Prices)
                .OrderBy(t => t.Position);

            // Génère la brochure au format Word
            var templatePath = Server.MapPath("~/Content/Bookmaker.xml");
            var word = QuickWord.Generate(travels, templatePath);

            // Enregistre la brochure Word
            // (quand on est sur http://localhost/)
            var file = "Bookmaker.doc";
            if (Request.Url.IsLoopback)
            {
                var path = Server.MapPath("~/App_Data/" + file);
                System.IO.File.WriteAllText(path, word);
            }

            // Renvoie la brochure au format Word
            return File(Encoding.UTF8.GetBytes(word), "application/msword", file);
        }

        //
        // GET: /Booklets/JsonExport

        public FileResult JsonExport()
        {
            // Retrouve toutes les brochures classées par année
            var booklets = db
                .Booklets
                .Include(b => b.Travels)
                .OrderBy(b => b.Year)
                .ThenBy(b => b.Title).ToList();

            // Pour chaque brochure
            foreach (var booklet in booklets)
            {
                // Retrouve tous les voyages classés par position
                booklet.Travels = db
                    .Travels
                    .Include(t => t.Prices)
                    .Include(t => t.Sections)
                    .Where(t => t.Booklet_ID == booklet.Booklet_ID)
                    .OrderBy(t => t.Position).ToList();

                // Pour chaque voyage
                foreach (var travel in booklet.Travels)
                {
                    // Retrouve tous les tarifs classés par titre
                    travel.Prices = travel.Prices.OrderBy(p => p.Title).ToList();

                    // Retrouve toutes les parties classées par position
                    travel.Sections = travel.Sections.OrderBy(s => s.Position).ToList();
                }
            }

            // Transforme les données au format Json
            var json = ImportExport.JsonExport(booklets);

            // Sauvegarde les données au format Json
            // (quand on est sur http://localhost/)
            var file = "json_db.txt";
            if (Request.Url.IsLoopback)
            {
                var path = Server.MapPath("~/App_Data/" + file);
                System.IO.File.WriteAllText(path, json);
            }

            // Renvoie les données au format Json
            return File(Encoding.UTF8.GetBytes(json), "application/json", file);
        }

        //
        // GET: /Booklets/JsonImport

        public ActionResult JsonImport()
        {
            // Charge les données à importer
            var file = Server.MapPath("~/App_Data/json_db.txt");
            var json = System.IO.File.ReadAllText(file);
            var booklets = ImportExport.JsonImport(json);

            // Rien à faire quand rien n'a été importé
            if (booklets.Count == 0) return RedirectToAction("Index");

            // Vide les tables actuelles
            db.TruncateTable("Prices", "Price_ID");
            db.TruncateTable("Sections", "Section_ID");
            db.TruncateTable("Travels", "Travel_ID");
            db.TruncateTable("Booklets", "Booklet_ID");

            // Importe les données dans la base de données
            booklets.ForEach(t => db.Booklets.Add(t));

            // Insère les données importées dans la base de données
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
