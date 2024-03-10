using System;
using Unity;

using ProEstimator.Business.Payments;
using ProEstimator.DataRepositories.Panels;
using ProEstimator.DataRepositories.Panels.LinkRules;
using ProEstimator.Business.Logic.ChatBot;
using ProEstimator.Business.Logic;
using ProEstimator.Business.OptOut;
using ProEstimator.Business.Panels.LinkRules;
using ProEstimator.Business.Panels;
using ProEstimator.DataRepositories.Vendors;
using ProEstimator.DataRepositories.Contracts;
using ProEstimator.DataRepositories.ProAdvisor;
using ProEstimator.Business.ProAdvisor;

namespace ProEstimator.Admin
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // Register your type's mappings here.
            RegisterDataRepositories(container);
            RegisterServices(container);

            container.RegisterType<IPanelRepository, PanelRepository>();
            container.RegisterType<ILinkRuleLineRepository, LinkRuleLineRepository>();
            container.RegisterType<ILinkRulePresetLinkRepository, LinkRulePresetLinkRepository>();
            container.RegisterType<ILinkRuleRepository, LinkRuleRepository>();
        }

        private static void RegisterServices(IUnityContainer container)
        {
            container.RegisterType<IStripeService, StripeService>();
            container.RegisterType<IPaymentService, PaymentService>();
            container.RegisterType<IOptOutService, OptOutService>();
            container.RegisterType<IChatBotService, ChatBotService>();
            container.RegisterType<IEstimateService, EstimateService>();
            container.RegisterType<IPanelService, PanelService>();
            container.RegisterType<ILinkRuleService, LinkRuleService>();
            container.RegisterType<IProAdvisorProfileService, ProAdvisorProfileService>();
        }

        private static void RegisterDataRepositories(IUnityContainer container)
        {
            container.RegisterType<IPanelRepository, PanelRepository>();
            container.RegisterType<ILinkRuleLineRepository, LinkRuleLineRepository>();
            container.RegisterType<ILinkRulePresetLinkRepository, LinkRulePresetLinkRepository>();
            container.RegisterType<ILinkRuleRepository, LinkRuleRepository>();
            container.RegisterType<IInvoiceFailureSummaryRepository, InvoiceFailureSummaryRepository>();
            container.RegisterType<IProAdvisorPresetRepository, ProAdvisorPresetRepository>();
            container.RegisterType<IProAdvisorPresetShellRepository, ProAdvisorPresetShellRepository>();
            container.RegisterType<IProAdvisorProfileRepository, ProAdvisorProfileRepository>();
        }
    }
}