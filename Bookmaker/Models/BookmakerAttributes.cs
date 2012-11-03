using System;
using System.Web.Mvc;

namespace Bookmaker.Models
{
    /// <summary>
    /// Représente un attribut qui est utilisé pour déterminer si la mise à jour de la brochure en cours est possible
    /// </summary>
    public class BookletUpdatableAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Nom du paramètre où récupérer l'identifiant de la brochure en cours (Root_ID par défaut)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indique si l'action doit continuer lorsque la mise à jour de la brochure n'est pas possible (false par défaut pour renvoyer un statut HTTP 403 Forbidden)
        /// </summary>
        public bool Continue { get; set; }

        public BookletUpdatableAttribute()
        {
            if (string.IsNullOrEmpty(Name)) Name = "Root_ID";
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Vérifie si la brochure en cours est en lecture seule
            var db = new BookmakerContext();
            var Booklet_ID = Convert.ToInt32(filterContext.ActionParameters[Name]);
            var IsUpdatable = db.BookletIsUpdatable(Booklet_ID);

            // Si la mise à jour de la brochure n'est pas possible
            if (!IsUpdatable)
            {
                // Et que l'attribut n'a pas été paramétré pour continuer dans ce cas
                if (!Continue)
                {
                    // Alors on renvoie le statut HTTP Forbidden
                    filterContext.Result = new HttpStatusCodeResult(403);
                }
            }

            // Mémorise l'état de la brochure pour la suite de l'action
            filterContext.Controller.ViewBag.IsUpdatable = IsUpdatable;

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }
    }
}