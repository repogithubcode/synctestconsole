using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;

using ProEstimator.Business.Panels;

using ProEstimator.Admin.ViewModel.LinkRules;
using ProEstimator.Admin.ViewModelMappers.Panels;
using ProEstimator.Business.Panels.LinkRules;
using ProEstimator.Admin.ViewModelMappers.LinkRules;

namespace ProEstimator.Admin.Controllers
{
    public class LinkRulesController : AdminController
    {
        private IPanelService _panelService;
        private ILinkRuleService _linkRuleService;

        public LinkRulesController(IPanelService panelService, ILinkRuleService linkRuleService)
        {
            _panelService = panelService;
            _linkRuleService = linkRuleService;
        }

        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/LinkRules";
                return Redirect("/LogOut");
            }

            if (!ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.HasPermission(GetSalesRepID(), "AddOns"))
            {
                return Redirect("/");
            }

            return View();
        }

        public ActionResult GetPanelsForGrid([DataSourceRequest] DataSourceRequest request)
        {
            List<PanelsGridRowVM> rulesVMs = new List<PanelsGridRowVM>();

            if (AdminIsValidated())
            {
                List<Panel> panels = _panelService.GetMainPanels();

                foreach (Panel panel in panels)
                {
                    rulesVMs.Add(new PanelsGridRowVM() { PanelID = panel.ID, PanelName = panel.PanelName, Symmetry = panel.Symmetry });
                }

                rulesVMs = rulesVMs.OrderBy(o => o.PanelName).ToList();
            }

            return Json(rulesVMs.ToDataSourceResult(request));
        }

        public ActionResult GetLinkedPanelsForGrid([DataSourceRequest] DataSourceRequest request, int panelID)
        {
            List<PanelsGridRowVM> rulesVMs = new List<PanelsGridRowVM>();

            if (panelID > 0 && AdminIsValidated())
            {
                List<Panel> panels = _panelService.GetMainPanels().Where(o => o.ID != panelID).ToList();
                List<Panel> linkedPanels = _panelService.GetLinkedPanels(panelID);

                foreach (Panel panel in panels)
                {
                    bool selected = linkedPanels.FirstOrDefault(o => o.ID == panel.ID) != null;
                    rulesVMs.Add(new PanelsGridRowVM() { PanelID = panel.ID, PanelName = panel.PanelName, Symmetry = panel.Symmetry, Selected = selected });
                }
            }

            rulesVMs = rulesVMs.OrderBy(o => o.PanelName).ToList();

            return Json(rulesVMs.ToDataSourceResult(request));
        }

        public JsonResult GetPanelDetails(int panelID)
        {
            PanelDetailsResult result = new PanelDetailsResult();

            Panel panel = _panelService.GetPanel(panelID);

            if (panel == null)
            {
                result.Success = false;
                result.ErrorMessage = "Invalid Panel ID";
            }
            else
            {
                PanelDetailsVMMapper mapper = new PanelDetailsVMMapper();
                PanelDetailsVM vm = mapper.Map(new PanelDetailsVMMapperConfiguration() { Panel = panel, LinkRuleService = _linkRuleService });

                result.PanelDetails = vm;
                result.Success = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SavePanelDetails(int panelID, string panelName, int sortOrder, bool symmetry, int sectionLinkRuleID, int primaryPanelLinkRuleID, string selectedLinkedIDs)
        {
            if (!AdminIsValidated())
            {
                return Json(new FunctionResult(false, "Access denied"), JsonRequestBehavior.AllowGet);
            }

            FunctionResult saveResult = _panelService.SavePanelDetails(ActiveLogin.ID, panelID, panelName, sortOrder, symmetry, sectionLinkRuleID, primaryPanelLinkRuleID, selectedLinkedIDs);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLinkRuleDetails(int id)
        {
            if (!AdminIsValidated())
            {
                return Json(new FunctionResult(false, "Access denied"), JsonRequestBehavior.AllowGet);
            }

            LinkRule linkRule = _linkRuleService.GetRule(id);
            if (linkRule == null)
            {
                return Json(new FunctionResult(false, "Invalid ID"), JsonRequestBehavior.AllowGet);
            }

            LinkRuleVMMapper mapper = new LinkRuleVMMapper();
            LinkRuleVM vm = mapper.Map(new LinkRuleVMMapperConfiguration() { LinkRuleService = _linkRuleService, LinkRule = linkRule });
            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveMatchText(int ruleID, string id, int sortOrder, int matchPiece, int childRuleID, int matchType, string matchText, bool indented, bool removed, bool disabled)
        {
            if (!AdminIsValidated())
            {
                return Json(new FunctionResult(false, "Access denied"), JsonRequestBehavior.AllowGet);
            }

            int idInt = InputHelper.GetInteger(id);

            FunctionResultInt saveResult = _linkRuleService.SaveRuleLine(ActiveLogin.ID, ruleID, idInt, sortOrder, matchText, (MatchPiece)matchPiece, childRuleID, (MatchType)matchType, indented, removed, disabled);
            return Json(saveResult.Value, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMatchDetails(int panelID, int ruleTypeID)
        {
            Panel panel = _panelService.GetPanel(panelID);
            if (panel != null)
            {
                LinkRuleType linkRuleType = (LinkRuleType)ruleTypeID;
                if (linkRuleType == LinkRuleType.SectionContainer)
                {
                    return Json(_linkRuleService.GetMatchDetailsForRule(panel, LinkRuleType.SectionContainer), JsonRequestBehavior.AllowGet);
                }
                else if (linkRuleType == LinkRuleType.PrimarySection)
                {
                    return Json(_linkRuleService.GetMatchDetailsForRule(panel, LinkRuleType.PrimarySection), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(_linkRuleService.GetMatchDetailsForRule(panel, LinkRuleType.PrimaryPanel), JsonRequestBehavior.AllowGet);
                }
            }

            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteLinkRule(int panelID, int ruleTypeID)
        {
            LinkRuleType linkRuleType = (LinkRuleType)ruleTypeID;
            FunctionResult functionResult = _panelService.DeleteRuleFromPanel(ActiveLogin.ID, panelID, linkRuleType);

            return Json(new PanelDetailsResult(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSections([DataSourceRequest] DataSourceRequest request, string serviceBarcode, int year)
        {
            List<SectionVM> vms = new List<SectionVM>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ServiceBarcode", serviceBarcode));
            parameters.Add(new SqlParameter("Year", year));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("AddOnManager_GetSections", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                vms.Add(new SectionVM(row));
            }

            return Json(vms.ToDataSourceResult(request));
        }

        public ActionResult GetSectionParts([DataSourceRequest] DataSourceRequest request, int nheader, int nsection, string serviceBarcode, int year)
        {
            List<PartDetailsVM> vms = new List<PartDetailsVM>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("nHeader", nheader));
            parameters.Add(new SqlParameter("nSection", nsection));
            parameters.Add(new SqlParameter("ServiceBarCode", serviceBarcode));
            parameters.Add(new SqlParameter("Year", year));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("AddOnManager_GetSectionParts", parameters);

            PartDetailsVMMapper mapper = new PartDetailsVMMapper();

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                vms.Add(mapper.Map(new PartDetailsVMMapperConfiguration() { Row = row }));
            }

            return Json(vms.ToDataSourceResult(request));
        }
    }
}