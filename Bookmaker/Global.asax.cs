using System;
using System.Web.Mvc;
using System.Web.Routing;
using Bookmaker.Helpers;
using Bookmaker.Models;
using StackExchange.Profiling;

namespace Bookmaker
{
    // Remarque : pour obtenir des instructions sur l'activation du mode classique IIS6 ou IIS7,
    // visitez http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            ModelBinders.Binders.Add(typeof(string), new StringModelBinder());

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            AutoMap.Configure();

            MiniProfilerEF.Initialize();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Request.IsLocal) MiniProfiler.Start();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            MiniProfiler.Stop();
        }
    }
}