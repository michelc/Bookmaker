﻿using System.Web.Mvc;
using System.Web.Routing;

namespace Bookmaker
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.LowercaseUrls = true;

            routes.MapRoute(
                name: "Default",
                url: "{root_id}/{controller}/{action}/{id}",
                defaults: new { root_id = "0", controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}