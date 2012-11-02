using System;
using System.Web.Mvc;

namespace Bookmaker.Models
{
    public class BookletUpdatableAttribute : ActionFilterAttribute
    {
        public string ParamKey { get; set; }

        public BookletUpdatableAttribute()
        {
            if (string.IsNullOrEmpty(this.ParamKey)) ParamKey = "Root_ID";
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var Booklet_ID = Convert.ToInt32(filterContext.ActionParameters[ParamKey]);

            var db = new BookmakerContext();
            var IsUpdatable = db.BookletIsUpdatable(Booklet_ID);

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