using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AutoMapper.QueryableExtensions;
using Bookmaker.Helpers;
using Bookmaker.Models;

namespace Bookmaker.Controllers
{
    public class PricesController : Controller
    {
        private BookmakerContext db = new BookmakerContext();

        //
        // GET: /Prices

        public ViewResult Index(int root_id)
        {
            // Retrouve tous les tarifs de la brochure
            var prices = db
                .Prices
                .Include(p => p.Travel)
                .Where(p => p.Travel.Booklet_ID == root_id)
                .OrderBy(p => p.Travel.Position)
                .ThenBy(p => p.Price1)
                .Project().To<PriceIndex>()
                .ToList();

            return View("List", prices);
        }

        //
        // GET: /Prices/Details/5

        public ViewResult Details(int id)
        {
            var price = db.Prices.Find(id);
            price.Travel = db.Travels.Find(price.Travel_ID);

            return View(price);
        }

        //
        // GET: /Prices/Create?int Parent_ID=5

        public ViewResult Create(int Parent_ID)
        {
            var price = new Price
            {
                Travel_ID = Parent_ID,
                Travel = db.Travels.Find(Parent_ID)
            };

            return View(price);
        }

        //
        // POST: /Prices/Create

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Price price)
        {
            if (ModelState.IsValid)
            {
                db.Prices.Add(price);
                db.SaveChanges();

                this.Flash(string.Format("Le tarif {0} a été créé", price.Title));
                return RedirectToAction("Details", "Travels", new { id = price.Travel_ID });
            }

            price.Travel = db.Travels.Find(price.Travel_ID);
            return View(price);
        }

        //
        // GET: /Prices/Edit/5

        public ActionResult Edit(int id)
        {
            var price = db.Prices.Find(id);
            price.Travel = db.Travels.Find(price.Travel_ID);

            return View(price);
        }

        //
        // POST: /Prices/Edit/5

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Price price)
        {
            if (ModelState.IsValid)
            {
                db.Entry(price).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash(string.Format("Le tarif {0} a été modifié", price.Title));
                return RedirectToAction("Details", "Travels", new { id = price.Travel_ID });
            }

            price.Travel = db.Travels.Find(price.Travel_ID);
            return View(price);
        }

        //
        // GET: /Prices/Delete/5

        public ActionResult Delete(int id)
        {
            var price = db.Prices.Find(id);
            price.Travel = db.Travels.Find(price.Travel_ID);

            return View(price);
        }

        //
        // POST: /Prices/Delete/5

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var price = db.Prices.Find(id);
            db.Prices.Remove(price);
            db.SaveChanges();

            this.Flash(string.Format("Le tarif {0} a été supprimé", price.Title));
            return RedirectToAction("Details", "Travels", new { id = price.Travel_ID });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}