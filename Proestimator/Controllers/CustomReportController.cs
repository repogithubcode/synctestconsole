using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using ProEstimatorData.DataModel;

using Proestimator.ViewModel.CustomReports;

using ProEstimator.Business.Logic;
using ProEstimatorData;

namespace Proestimator.Controllers
{
    public class CustomReportController : SiteController
    {
        [HttpGet]
        [Route("{userID}/custom-report/{reportID}")]
        public ActionResult Edit(int userID, int reportID)
        {
            RedirectResult redirectResult = DoRedirect("");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            CustomReportTemplateVM customReportTemplateVM = null;

            CustomReportTemplate template = CustomReportTemplate.Get(reportID);
            if (template != null && (template.LoginID == activeLogin.LoginID) || template.IsSystemReport)
            {
                customReportTemplateVM = new CustomReportTemplateVM(template);
            }

            if (customReportTemplateVM == null)
            {
                customReportTemplateVM = new CustomReportTemplateVM();
                customReportTemplateVM.UserID = userID;
                customReportTemplateVM.LoginID = activeLogin.LoginID;
            }

            customReportTemplateVM.IsRegularReport = template.CustomReportTemplateType == CustomReportTemplateType.Report;

            ViewBag.NavID = "reports";
            ViewBag.settingsTopMenuID = 5;
            ViewBag.UserID = userID;

            customReportTemplateVM.UserID = userID;
            customReportTemplateVM.LoginID = activeLogin.LoginID;
            customReportTemplateVM.Tags = CustomReportTemplate.GetAllTags();
            customReportTemplateVM.TemplateID = reportID;

            customReportTemplateVM = GetReportHeaderFooterSelections(customReportTemplateVM);

            return View(customReportTemplateVM);
        }

        [HttpPost]
        [Route("{userID}/custom-report/{reportID}")]
        public ActionResult Edit(CustomReportTemplateVM customReportTemplateVM)
        {
            RedirectResult redirectResult = DoRedirect("");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            CustomReportTemplate template = CustomReportTemplate.Get(customReportTemplateVM.ID);

            if (template == null)
            {
                template = new CustomReportTemplate();

                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(customReportTemplateVM.UserID, GetComputerKey());
                int loginID = activeLogin.LoginID;

                template.LoginID = loginID;
            }

            template.Name = customReportTemplateVM.Name;
            template.Description = customReportTemplateVM.Description;
            template.Template = customReportTemplateVM.Template;
            template.IsActive = customReportTemplateVM.IsActive;
            template.IsDeleted = customReportTemplateVM.IsDeleted;
            template.ReportHeaderID = customReportTemplateVM.SelectedReportHeaderID;
            template.ReportFooterID = customReportTemplateVM.SelectedReportFooterID;
            template.IsSystemReport = customReportTemplateVM.IsSystemReport;

            SuccessBoxFeatureLog.LogFeature(customReportTemplateVM.LoginID, SuccessBoxModule.Report, "Creating custom report", GetActiveLoginID(customReportTemplateVM.UserID));
            SaveResult saveResult = template.Save();

            customReportTemplateVM.ID = template.ID;

            return DoRedirect("/" + customReportTemplateVM.UserID + "/custom-report/" + customReportTemplateVM.ID);
        }

        [HttpGet]
        [Route("{userID}/new-custom-report/{reportType}")]
        public ActionResult NewCustomReport(int userID, int reportType)
        {
            CustomReportTemplate template = new CustomReportTemplate();
            template.LoginID = ActiveLogin.LoginID;
            template.CustomReportTemplateType = (CustomReportTemplateType)reportType;
            template.IsActive = false;
            template.Save(ActiveLogin.ID);

            return DoRedirect("/" + userID + "/custom-report/" + template.ID);
        }

        private CustomReportTemplateVM GetReportHeaderFooterSelections(CustomReportTemplateVM customReportTemplateVM)
        {
            LoginInfo loginInfo = LoginInfo.GetByID(customReportTemplateVM.LoginID);

            Boolean isStaffAccount = loginInfo.StaffAccount;
            ViewBag.IsStaffAccount = isStaffAccount;

            if (customReportTemplateVM.IsRegularReport)
            {
                // merge myReport and systemReportHeader templates
                List <CustomReportTemplate> customReportTemplateColl = CustomReportTemplate.GetForLogin(customReportTemplateVM.LoginID);

                var reportHeaderTemplates = customReportTemplateColl.Where(reportTemplate => reportTemplate.CustomReportTemplateType == CustomReportTemplateType.Header).
                                            Select(reportTemplate => new SelectListItem() { Text = reportTemplate.Name, Value = reportTemplate.ID.ToString() }).ToList();

                var reportFooterTemplates = customReportTemplateColl.Where(reportTemplate => reportTemplate.CustomReportTemplateType == CustomReportTemplateType.Footer).
                                            Select(reportTemplate => new SelectListItem() { Text = reportTemplate.Name, Value = reportTemplate.ID.ToString() }).ToList();

                reportHeaderTemplates.Insert(0, new SelectListItem() { Text = "--Select--", Value = "0" });
                reportFooterTemplates.Insert(0, new SelectListItem() { Text = "--Select--", Value = "0" });

                customReportTemplateVM.ReportHeaderSelections = new SelectList(reportHeaderTemplates, "Value", "Text");
                customReportTemplateVM.ReportFooterSelections = new SelectList(reportFooterTemplates, "Value", "Text");
            }

            return customReportTemplateVM;
        }

        public ActionResult GetCustomReportTemplateList([DataSourceRequest] DataSourceRequest request, int loginID, bool showDeleted, int customReportTemplateTypeID, string reportType)
        {
            List<CustomReportTemplateVM> lines = new List<CustomReportTemplateVM>();

            Boolean isSystemReport = false;
            if (reportType == "systemreport")
            {
                isSystemReport = true;
            }

            CustomReportTemplateType templateType = (CustomReportTemplateType)customReportTemplateTypeID;

            List<CustomReportTemplate> templates = CustomReportTemplate.GetForLogin(loginID, showDeleted);
            templates = templates.Where(o => o.IsSystemReport == isSystemReport && o.CustomReportTemplateType == templateType).ToList();

            foreach (CustomReportTemplate template in templates)
            {
                CustomReportTemplateVM vm = new CustomReportTemplateVM(template);

                if (vm.IsDeleted)
                {
                    vm.DeleteRestoreImgName = "restore.gif";
                }
                else
                {
                    vm.DeleteRestoreImgName = "delete.gif";
                }

                lines.Add(vm);
            }

            return Json(lines.ToDataSourceResult(request));
        }

        public JsonResult GenerateReportTest(int templateID)
        {
            CustomReportGenerator generator = new CustomReportGenerator();
            ReportFunctionResult result = generator.RenderCustomReport(templateID, 0);
            return Json(result.ErrorMessage, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteMyCustomReport(int loginID, int reportID)
        {
            CustomReportTemplate template = CustomReportTemplate.Get(reportID);

            FunctionResult result = new FunctionResult();

            if (template.LoginID != loginID)
            {
                result.ErrorMessage = @Proestimator.Resources.ProStrings.ErrorDeletingReportInvalidReportID;
            }
            else
            {
                CustomReportTemplate.DeleteRestoreCustomReport(reportID, true);
            }

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                result.Success = false;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region View Deleted Custom Reports

        public JsonResult RestoreMyCustomReport(int userID, int reportID)
        {
            if (IsUserAuthorized(userID))
            {
                CustomReportTemplate template = CustomReportTemplate.Get(reportID);

                FunctionResult result = new FunctionResult();

                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                if (template.LoginID != activeLogin.LoginID)
                {
                    result.ErrorMessage = @Proestimator.Resources.ProStrings.ErrorDeletingReportInvalidReportID;
                }
                else
                {
                    CustomReportTemplate.DeleteRestoreCustomReport(reportID, false);
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    result.Success = false;
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new FunctionResult("Unauthorized Login"), JsonRequestBehavior.AllowGet);
        }

        #endregion

        [HttpGet]
        [Route("{userID}/customreport/copy/{reportID}")]
        public ActionResult CopyMyCustomReport(int userID, int reportID)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            LoginInfo loginInfo = LoginInfo.GetByID(activeLogin.LoginID);

            int newID = CustomReportTemplate.CopyCustomReport(activeLogin.LoginID, reportID);
            return DoRedirect("/" + userID + "/custom-report/" + newID);
        }
    }
}