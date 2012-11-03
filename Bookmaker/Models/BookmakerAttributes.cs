using System;
using System.Web.Mvc;

namespace Bookmaker.Models
{
    public class BookletUpdatableAttribute : ActionFilterAttribute
    {
        public string ParamKey { get; set; }
        public bool Continue { get; set; }

        public BookletUpdatableAttribute()
        {
            if (string.IsNullOrEmpty(ParamKey)) ParamKey = "Root_ID";
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var Booklet_ID = Convert.ToInt32(filterContext.ActionParameters[ParamKey]);

            var db = new BookmakerContext();
            var IsUpdatable = db.BookletIsUpdatable(Booklet_ID);

            if (!IsUpdatable) {
                if (!Continue)
                {
                    filterContext.Result = new HttpStatusCodeResult(403);
                }
            }

            filterContext.RouteData.Values.Add("is_updatable", IsUpdatable);
            filterContext.Controller.ViewBag.IsUpdatable = IsUpdatable;

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }
    }
}