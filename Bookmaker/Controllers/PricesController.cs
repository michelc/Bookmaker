using System.Data;
using System.Web.Mvc;
using Bookmaker.Helpers;
using Bookmaker.Models;

namespace Bookmaker.Controllers
{
    public class PricesController : Controller
    {
        private BookmakerContext db = new BookmakerContext();

        //
        // GET: /Prices/Details/5

        public ViewResult Details(int id)
        {
            var price = db.Prices.Find(id);

            return View(price);
        }

        //
        // GET: /Prices/Create?int ParentID=5

        public ViewResult Create(int ParentID)
        {
            var price = new Price
            {
                TravelID = ParentID,
                Travel = db.Travels.Find(ParentID)
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

                this.Flash(string.Format("Le tarif {0}/{1} a été créé", price.Year, price.Title));
                return RedirectToAction("Details", "Travels", new { id = price.TravelID }); 
            }

            price.Travel = db.Travels.Find(price.TravelID);
            return View(price);
        }

        //
        // GET: /Prices/Edit/5

        public ActionResult Edit(int id)
        {
            var price = db.Prices.Find(id);
            price.Travel = db.Travels.Find(price.TravelID);

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

                this.Flash(string.Format("Le tarif {0}/{1} a été modifié", price.Year, price.Title));
                return RedirectToAction("Details", "Travels", new { id = price.TravelID });
            }

            price.Travel = db.Travels.Find(price.TravelID);
            return View(price);
        }

        //
        // GET: /Prices/Delete/5

        public ActionResult Delete(int id)
        {
            var price = db.Prices.Find(id);

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

            this.Flash(string.Format("Le tarif {0}/{1} a été supprimé", price.Year, price.Title));
            return RedirectToAction("Details", "Travels", new { id = price.TravelID });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
