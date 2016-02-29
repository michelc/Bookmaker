using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Bookmaker.Helpers;
using Bookmaker.Models;

namespace Bookmaker.Controllers
{
    public class PricesController : Controller
    {
        private BookmakerContext db = new BookmakerContext();

        //
        // GET: /Prices

        [BookletUpdatable(Continue=true)]
        public ViewResult Index(int root_id)
        {
            // Retrouve tous les tarifs de la brochure
            var prices = db
                .Prices
                .Where(p => p.Travel.Booklet_ID == root_id)
                .OrderBy(p => p.Travel.Position)
                .ThenBy(p => p.Price1)
                .MapTo<PriceIndex>()
                .ToList();

            return View("List", prices);
        }

        //
        // GET: /Prices/Export

        public FileResult Export(int root_id)
        {
            // Retrouve tous les tarifs de la brochure
            var prices = db
                .Prices
                .Where(p => p.Travel.Booklet_ID == root_id)
                .OrderBy(p => p.Travel.Position)
                .ThenBy(p => p.Price1)
                .MapTo<PriceIndex>()
                .ToList();

            var csv = new StringBuilder();
            var sep = ";";
            var current = -1;
            foreach (var price in prices)
            {
                csv.Append("\"");
                if (current != price.Travel_ID)
                {
                    current = price.Travel_ID;
                    csv.Append(price.TravelTitle.Replace("[br]", " / ").Replace("\"", "'"));
                }
                csv.Append("\"");
                csv.Append(sep);
                csv.Append("\"");
                csv.Append(price.Title.Replace("\"", "'"));
                csv.Append("\"");
                csv.Append(sep);
                csv.Append(price.Price1);
                csv.Append(sep);
                csv.Append(price.Price2);
                csv.Append(sep);
                csv.Append(price.Price3);
                csv.Append(sep);
                csv.Append(price.Price4);
                csv.Append(sep);
                csv.Append(price.Price5);
                csv.AppendLine();
            }

            // Renvoie les données au format CSV
            var file = "Bookmaker.csv";
            // application/vnd.ms-excel
            string text = csv.ToString();
            Response.ContentEncoding = Encoding.GetEncoding("iso-8859-1");
            return File(Encoding.Default.GetBytes(text), "text/csv", file);
        }

        //
        // GET: /Prices/Details/5

        [BookletUpdatable(Continue = true)]
        public ViewResult Details(int id)
        {
            var price = db.Prices.Find(id);
            price.Travel = db.Travels.Find(price.Travel_ID);

            return View(price);
        }

        //
        // GET: /Prices/Import?Parent_ID=5

        [BookletUpdatable()]
        public ActionResult Import(int Parent_ID)
        {
            var travel = db.Travels.Find(Parent_ID);

            var import = new PriceImport { Travel = travel };
            return View(import);
        }

        //
        // POST: /Prices/Import?Parent_ID=5

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable()]
        public ActionResult Import(int Parent_ID, string rawcontent)
        {
            var travel = db.Travels.Find(Parent_ID);

            if (ModelState.IsValid)
            {
                // Reformate et contrôle le contenu saisi
                travel.Prices = InputHelper.PriceImport(rawcontent);

                // Enregistre les modifications
                db.Entry(travel).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash("Le voyage {0} a été initialisé", travel.Title);
                return RedirectToAction("Details", "Travels", new { id = travel.Travel_ID });
            }

            var import = new PriceImport { Travel = travel, RawContent = rawcontent };
            return View(import);
        }

        //
        // GET: /Prices/Create?int Parent_ID=5

        [BookletUpdatable()]
        public ActionResult Create(int Parent_ID)
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

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable()]
        public ActionResult Create(Price price)
        {
            if (ModelState.IsValid)
            {
                db.Prices.Add(price);
                db.SaveChanges();

                this.Flash("Le tarif {0} a été créé", price.Title);
                return RedirectToAction("Details", "Travels", new { id = price.Travel_ID });
            }

            price.Travel = db.Travels.Find(price.Travel_ID);
            return View(price);
        }

        //
        // GET: /Prices/Edit/5

        [BookletUpdatable()]
        public ActionResult Edit(int id, string view_from)
        {
            var price = db.Prices.Find(id);
            price.Travel = db.Travels.Find(price.Travel_ID);

            ViewBag.Cancel = null;
            if (view_from == "index")
            {
                view_from = string.Format("<a href=\"{0}\" class=\"cancel\">Annuler</a>", Url.Action("Index"));
                ViewBag.Cancel = new MvcHtmlString(view_from);
            }

            return View(price);
        }

        //
        // POST: /Prices/Edit/5

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable()]
        public ActionResult Edit(Price price, string view_from)
        {
            if (ModelState.IsValid)
            {
                db.Entry(price).State = EntityState.Modified;
                db.SaveChanges();

                this.Flash("Le tarif {0} a été modifié", price.Title);

                if (view_from == "index") return RedirectToAction("Index");

                return RedirectToAction("Details", "Travels", new { id = price.Travel_ID });
            }

            price.Travel = db.Travels.Find(price.Travel_ID);
            return View(price);
        }

        //
        // GET: /Prices/Delete/5

        [BookletUpdatable()]
        public ActionResult Delete(int id)
        {
            var price = db.Prices.Find(id);
            price.Travel = db.Travels.Find(price.Travel_ID);

            return View(price);
        }

        //
        // POST: /Prices/Delete/5

        [HttpPost, ValidateAntiForgeryToken, BookletUpdatable(), ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var price = db.Prices.Find(id);
            db.Prices.Remove(price);
            db.SaveChanges();

            this.Flash("Le tarif {0} a été supprimé", price.Title);
            return RedirectToAction("Details", "Travels", new { id = price.Travel_ID });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}