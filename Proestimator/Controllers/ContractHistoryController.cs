using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimatorData;
using ProEstimatorData.DataModel.Contracts;

using Proestimator.ViewModel.Contracts;
using Proestimator.ViewModelMappers.Contracts;
using Proestimator.ViewModel;
using ProEstimatorData.DataModel;

namespace Proestimator.Controllers
{
    public class ContractHistoryController : SiteController
    {
        [HttpGet]
        [Route("{userID}/settings/contract-history")]
        public ActionResult Index(int userID)
        {
            ViewBag.NavID = "settings";
            ViewBag.settingsTopMenuID = 3;

            ContractHistoryTabVMMapper mapper = new ContractHistoryTabVMMapper();
            ContractHistoryTabVM vm = mapper.Map(new ContractHistoryTabVMMapperConfiguration() { LoginID = ActiveLogin.LoginID });

            return View(vm);
        }

        [HttpPost]
        [Route("{userID}/settings/contract-history")]
        public ActionResult ContractPage()
        {
            return DoRedirect("/" + ActiveLogin.SiteUserID + "/settings/contract-history");
        }

        public JsonResult GetContractDetails(int userID, int contractID)
        {
            CacheActiveLoginID(userID);

            string errorMessage = "";

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                Contract contract = Contract.Get(contractID);

                if (contract != null && contract.LoginID == activeLogin.LoginID)
                {
                    ContractVMMapper contractMapper = new ContractVMMapper();
                    ContractVM contractVM = contractMapper.Map(new ContractVMMapperConfiguration() { Contract = contract });
                    return Json(contractVM, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    errorMessage = "Invalid contract";
                }
            }
            else
            {
                errorMessage = "Invalid login";
            }

            return Json(errorMessage, JsonRequestBehavior.AllowGet);
        }
    }
}