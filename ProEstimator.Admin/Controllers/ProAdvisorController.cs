using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;
using ProEstimatorData.DataModel.ProAdvisor;

using ProEstimator.Business.Panels;
using ProEstimator.Business.Panels.LinkRules;
using ProEstimator.Admin.ViewModel.LinkRules;
using ProEstimator.DataRepositories.ProAdvisor;

namespace ProEstimator.Admin.Controllers
{
    public class ProAdvisorController : AdminController
    {

        private IPanelService _panelService;
        private ILinkRuleService _linkRuleService;
        private IProAdvisorPresetShellRepository _proAdvisorPresetShellRepository;

        public ProAdvisorController(IPanelService panelService, ILinkRuleService linkRuleService, IProAdvisorPresetShellRepository proAdvisorPresetShellRepository)
        {
            _panelService = panelService;
            _linkRuleService = linkRuleService;
            _proAdvisorPresetShellRepository = proAdvisorPresetShellRepository;
        }

        // GET: ProAdvisor
        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/ProAdvisor";
                return Redirect("/LogOut");
            }

            if (!ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.HasPermission(GetSalesRepID(), "AddOns"))
            {
                return Redirect("/");
            }

            return View();
        }

        public ActionResult GetPresetsForPanel([DataSourceRequest] DataSourceRequest request, int panelID, string addAction, bool assigned, string name, string operationType, string laborType, bool sublet)
        {
            List<PresetGridRowVM> rows = new List<PresetGridRowVM>();

            if (AdminIsValidated() && panelID > 0)
            {
                Panel panel = _panelService.GetPanel(panelID);
                if (panel != null && panel.PrimaryPanelLinkRuleID > 0)
                {
                    List<ProAdvisorPresetShell> allPresets = _proAdvisorPresetShellRepository.GetAll().Where(o => !o.Deleted).OrderBy(o => o.Name).ToList();
                    List<LinkRulePresetLink> links = _linkRuleService.GetPresetLinksForRule(panel.PrimaryPanelLinkRuleID, addAction);

                    foreach (ProAdvisorPresetShell preset in allPresets)
                    {
                        if ((assigned && links.FirstOrDefault(o => o.PresetID == preset.ID) != null)
                            || (!assigned && links.FirstOrDefault(o => o.PresetID == preset.ID) == null))
                        {
                            if (
                               (string.IsNullOrEmpty(name) || preset.Name.ToLower().Contains(name))
                               && (operationType == "All" || preset.OperationType == operationType)
                               && (laborType == "All" || preset.LaborType == laborType)
                               && (!sublet || preset.Sublet)
                            )
                            {
                                rows.Add(new PresetGridRowVM(preset));
                            }
                        }
                    }
                }
            }

            return Json(rows.ToDataSourceResult(request));
        }

        public JsonResult AssignPresets(int panelID, string addAction, string ids)
        {
            if (AdminIsValidated() && panelID > 0)
            {
                Panel panel = _panelService.GetPanel(panelID);
                if (panel != null && panel.PrimaryPanelLinkRuleID > 0)
                {
                    string[] pieces = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    foreach (string piece in pieces)
                    {
                        LinkRulePresetLink link = new LinkRulePresetLink();
                        link.RuleID = panel.PrimaryPanelLinkRuleID;
                        link.PresetID = InputHelper.GetInteger(piece);
                        link.AddAction = addAction;
                        link.Save(ActiveLogin.ID);
                    }
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult UnassignPresets(int panelID, string addAction, string ids)
        {
            if (AdminIsValidated() && panelID > 0)
            {
                Panel panel = _panelService.GetPanel(panelID);
                if (panel != null && panel.PrimaryPanelLinkRuleID > 0)
                {
                    string[] pieces = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    List<LinkRulePresetLink> links = _linkRuleService.GetPresetLinksForRule(panel.PrimaryPanelLinkRuleID);

                    foreach (string piece in pieces)
                    {
                        int presetID = InputHelper.GetInteger(piece);

                        LinkRulePresetLink link = links.FirstOrDefault(o => o.PresetID == presetID && o.AddAction == addAction);
                        if (link != null)
                        {
                            _linkRuleService.DeletePresetLink(link);
                        }
                    }
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }
    }
}