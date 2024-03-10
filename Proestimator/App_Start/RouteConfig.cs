using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Proestimator
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "LoggedIn",
                url: "{userID}/{controller}/{action}/{id}",
                defaults: new { id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EstimatePages",
                url: "{userID}/estimate/{estimateID}/{action}/{id}",
                defaults: new { id = UrlParameter.Optional }
            );
        }
    }
}
