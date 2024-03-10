using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Proestimator
{
    public static class WebApiConfig
    {
        public static string UrlPrefix { get { return "api"; } }
        public static string UrlPrefixRelative { get { return "~/api"; } }

        public static void Register(HttpConfiguration config)
        {
            config.EnableCors();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: WebApiConfig.UrlPrefix + "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //Telerik.Reporting.Services.WebApi.ReportsControllerConfiguration.RegisterRoutes(config);
        }
    }
}