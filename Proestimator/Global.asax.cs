using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;
using System.Web.Caching;
using System.Web.SessionState;

using Unity;
using Unity.AspNet.Mvc;

using Telerik.Reporting.Services.WebApi;

using ProEstimator.Business.Payments;


namespace Proestimator
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ReportsControllerConfiguration.RegisterRoutes(GlobalConfiguration.Configuration);
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ProEstimatorData.Services.MigrationService.RunMigrations();
            ProEstimatorData.DataModel.Contracts.ContractTerms.LoadAll();
            ProEstimator.Business.Logic.SiteGlobalsManager.LoadData();

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.TurnOffCache();

            //var container = new UnityContainer();

            //container.RegisterType<IPaymentService, PaymentService>();

            //DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        protected void Session_Start()
        {
            Session.Timeout = 480;
        }

        protected void Application_PostAuthorizeRequest()
        {
            if (IsWebApiRequest())
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }

        private bool IsWebApiRequest()
        {
            return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith(WebApiConfig.UrlPrefixRelative);
        }

    }
}