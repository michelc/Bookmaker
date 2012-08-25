using System;
using System.Web.Mvc;
using System.Web.Routing;
using Bookmaker.Helpers;
using Bookmaker.Models;
using LowercaseRoutesMVC;
using StackExchange.Profiling;

namespace Bookmaker
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRouteLowercase(
                "Default", // Route name
                "{root_id}/{controller}/{action}/{id}", // URL with parameters
                new { root_id = "1", controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            ModelBinders.Binders.Add(typeof(string), new StringModelBinder());

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            AutoMapperConfiguration.Configure();

            MiniProfilerEF.Initialize(true);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Request.IsLocal)
            {
                MiniProfiler.Start();
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            MiniProfiler.Stop();
        }
    }
}