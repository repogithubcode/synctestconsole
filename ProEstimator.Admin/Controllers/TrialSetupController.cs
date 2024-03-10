using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimator.Admin.ViewModel;
using ProEstimator.Admin.ViewModel.Logins;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.Admin;
using ProEstimator.Business.Logic.Admin;
using System.Net;
using ProEstimator.Admin.ViewModel.Import;

namespace ProEstimator.Admin.Controllers
{
    public class TrialSetupController : AdminController
    {
        [Route("TrialSetup/List")]
        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/TrialSetup/List";
                return Redirect("/LogOut");
            }
            else
            {
                TrialSetupVM trialSetupVM = new TrialSetupVM();
                return View(trialSetupVM);
            }
        }

        public JsonResult GetActiveContractTrialForLogin(string loginId)
        {
            TrialSetupVM trialSetupVM = new TrialSetupVM();

            trialSetupVM.SessionSalesRepID = GetSalesRepID();

            int loginID = InputHelper.GetInteger(loginId);

            // Load organization info
            LoginInfo loginInfo = LoginInfo.GetByID(loginID);
            if (loginInfo != null)
            {
                trialSetupVM.IsAccountInPE = true;
                trialSetupVM.IsAccountInWE = false;
                trialSetupVM.LoginID = loginID;
                trialSetupVM.LoginName = loginInfo.LoginName;
                trialSetupVM.Organizationname = loginInfo.Organization;
                trialSetupVM.TrialEndDate = DateTime.Now.AddDays(14).ToShortDateString();

                DateTime? webEstExpiration = ContractManager.GetWebEstExpiration(loginID);
                if (webEstExpiration.HasValue)
                {
                    if (webEstExpiration.Value >= DateTime.Now)
                    {
                        trialSetupVM.TrialEndDate = webEstExpiration.Value.ToShortDateString();
                    }
                }

                SalesRep salesRep = SalesRep.Get(loginInfo.SalesRepID);
                if (salesRep != null)
                {
                    trialSetupVM.SalesRep = salesRep.SalesRepID + " - " + salesRep.FirstName + " " + salesRep.LastName;
                }

                Trial activeTrial = Trial.GetActive(loginID);
                if (activeTrial != null)
                {
                    trialSetupVM.IsActiveTrialExist = true;
                    trialSetupVM.TrialEndDate = activeTrial.EndDate.ToShortDateString();
                }

                Contract activeContract = Contract.GetActive(loginID);
                if (activeContract != null)
                {
                    trialSetupVM.IsActiveContractExist = true;
                }

                // It is when account exists in PE but it need to check in WE also
                if(!trialSetupVM.IsActiveTrialExist && !trialSetupVM.IsActiveContractExist)
                {
                    trialSetupVM = PrepareTrialSetVMForWE(loginID, trialSetupVM);
                }
            }
            else
            {
                trialSetupVM = PrepareTrialSetVMForWE(loginID, trialSetupVM);

                trialSetupVM.IsAccountInPE = false;
                
                trialSetupVM.IsActiveTrialExist = false;
            }

            return Json(trialSetupVM, JsonRequestBehavior.AllowGet);
        }

        private TrialSetupVM PrepareTrialSetVMForWE(int loginID, TrialSetupVM trialSetupVM)
        {
            WebEstAccountInfo webEstAccountInfo = WebEstAccountInfo.GetForFilter(loginID);

            trialSetupVM.IsAccountInWE = false;

            if (webEstAccountInfo != null)
            {
                trialSetupVM.LoginID = webEstAccountInfo.LoginID;
                trialSetupVM.LoginName = webEstAccountInfo.LoginName;
                trialSetupVM.Organizationname = webEstAccountInfo.Organization;
                trialSetupVM.SalesRep = webEstAccountInfo.SalesRepID + " - " + webEstAccountInfo.SalesRepFirstName + " " + webEstAccountInfo.SalesRepLastName;
                trialSetupVM.IsAccountInWE = true;

                trialSetupVM.IsActiveContractExist = true;
                trialSetupVM.TrialEndDate = DateTime.Now.AddDays(14).ToShortDateString();

                DateTime? webEstExpiration = ContractManager.GetWebEstExpiration(loginID);
                if (webEstExpiration.HasValue)
                {
                    if(webEstExpiration.Value >= DateTime.Now)
                    {
                        trialSetupVM.TrialEndDate = webEstExpiration.Value.ToShortDateString();
                    }
                }
            }

            return trialSetupVM;
        }

        public JsonResult ImportWithTrial(string loginId, string trialEndDate)
        {
            int loginID = InputHelper.GetInteger(loginId);

            AdminService adminService = new AdminService();
            var model = adminService.ImportLogin(loginId, 0, trialEndDate);

            Trial trial = Trial.GetActive(loginID);
            if (trial != null)
            {
                trial.HasEMS = true;
                trial.HasFrameData = true;
                trial.Save(ActiveLogin.ID);
            }

            ImportVM importVM = new ImportVM(model);
            importVM.ImpersonateLink = "https://" + System.Configuration.ConfigurationManager.AppSettings["server"] + "?impersonate=" + adminService.Encrypt(loginID);

            return Json(importVM, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ChangeTrial(string loginId, string trialEndDate)
        {
            int loginID = InputHelper.GetInteger(loginId);
            DateTime endDate = InputHelper.GetDateTime(trialEndDate);

            ImportVM importVM = new ImportVM();
            importVM.LoginID = loginID;
            importVM.Message = "";

            Trial trial = Trial.GetActive(loginID);
            if (trial != null)
            {
                trial.EndDate = endDate;
                SaveResult saveResult = trial.Save(ActiveLogin.ID);
                if (!saveResult.Success)
                {
                    importVM.Message = saveResult.ErrorMessage;
                }
                else
                {
                    importVM.Message = "Trial end date changed";
                }
            }

            AdminService adminService = new AdminService();
            importVM.ImpersonateLink = "https://" + System.Configuration.ConfigurationManager.AppSettings["server"] + "?impersonate=" + adminService.Encrypt(loginID);

            return Json(importVM, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateTrial(string loginId, string trialEndDate)
        {
            int loginID = InputHelper.GetInteger(loginId);
            AdminService adminService = new AdminService();

            DateTime startDate = DateTime.Now.Date;
            DateTime endDate = InputHelper.GetDateTime(trialEndDate);

            ContractManager.CreateTrial(loginID, startDate, endDate, true, true, false, true, true, true, false, false);

            // Load organization info
            LoginInfo loginInfo = LoginInfo.GetByID(loginID);

            ImportVM importVM = new ImportVM();
            importVM.LoginID = InputHelper.GetInteger(loginId);
            importVM.LoginName = loginInfo.LoginName;
            importVM.LoginImported = true;
            importVM.Message = "Trial records created successfully";

            importVM.ImpersonateLink = "https://" + System.Configuration.ConfigurationManager.AppSettings["server"] + "?impersonate=" + adminService.Encrypt(loginID);

            return Json(importVM, JsonRequestBehavior.AllowGet);
        }
    }
}