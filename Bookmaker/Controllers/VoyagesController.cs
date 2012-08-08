using System.Data;
using System.Linq;
using System.Web.Mvc;
using Bookmaker.Helpers;
using Bookmaker.Models;

namespace Bookmaker.Controllers
{
    public class VoyagesController : Controller
    {
        private BookmakerContext db = new BookmakerContext();

        //
        // GET: /Voyages/

        public ViewResult Index()
        {
            var voyages = db
                .Voyages
                .OrderBy(voyage => voyage.Position)
                .ThenBy(voyage => voyage.Title)
                .Select(voyage => new VoyageIndex
                {
                    VoyageID = voyage.VoyageID,
                    Position = voyage.Position,
                    Title = voyage.Title,
                    VoyageType = voyage.VoyageType,
                    PricesCount = voyage.Prices.Count(),
                    PartiesCount = voyage.Parties.Count()
                }).ToList();

            return View(voyages);
        }

        //
        // GET: /Voyages/Details/5

        public ViewResult Details(int id)
        {
            var voyage = db.Voyages.Find(id);

            return View(voyage);
        }

        //
        // GET: /Voyages/Create

        public ViewResult Create()
        {
            var voyage = new Voyage();

            ViewBag.VoyageType = db.Enums<VoyageType>();
            return View(voyage);
        }

        //
        // POST: /Voyages/Create

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Voyage voyage)
        {
            if (ModelState.IsValid)
            {
                db.Voyages.Add(voyage);
                db.SaveChanges();

                this.Flash(string.Format("Le voyage {0} a été créé", voyage.Title));
                return RedirectToAction("Details", new { id = voyage.VoyageID });
            }

            ViewBag.VoyageType = db.Enums<VoyageType>();
            return View(voyage);
        }

        //
        // GET: /Voyages/Edit/5

        public ViewResult Edit(int id)
        {
            ViewBag.VoyageType = db.Enums<VoyageType>();
            var voyage = db.Voyages.Find(id);

            return View(voyage);
        }

        //
        // POST: /Voyages/Edit/5

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Voyage voyage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(voyage).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash(string.Format("Le voyage {0} a été modifié", voyage.Title));
                return RedirectToAction("Details", new { id = voyage.VoyageID });
            }

            ViewBag.VoyageType = db.Enums<VoyageType>();
            return View(voyage);
        }

        //
        // GET: /Voyages/Delete/5

        public ViewResult Delete(int id)
        {
            var voyage = db.Voyages.Find(id);

            return View(voyage);
        }

        //
        // POST: /Voyages/Delete/5

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var voyage = db.Voyages.Find(id);
            db.Voyages.Remove(voyage);
            db.SaveChanges();

            this.Flash(string.Format("Le voyage {0} a été supprimé", voyage.Title));
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
