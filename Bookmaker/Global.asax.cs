using System;
using System.Web.Mvc;
using System.Web.Routing;
using Bookmaker.Helpers;
using Bookmaker.Models;
using StackExchange.Profiling;
using StackExchange.Profiling.EntityFramework6;

namespace Bookmaker
{
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

            MiniProfilerEF6.Initialize();
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
