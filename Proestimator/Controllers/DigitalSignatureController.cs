using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Logic;

using Proestimator.ViewModel.Contracts;
using Proestimator.ViewModelMappers.Contracts;

namespace Proestimator.Controllers
{
    public class DigitalSignatureController : SiteController
    {
        [HttpGet]
        [Route("{userID}/digital-signature/{contractID}")]
        public ActionResult SignContract(int userID, int contractID)
        {
            SignContractVMMapper mapper = new SignContractVMMapper();
            SignContractVM vm = mapper.Map(new SignContractVMMapperConfiguration() { LoginID = ActiveLogin.LoginID, ContractID = contractID });

            return View(vm);
        }

        [HttpPost]
        [Route("{userID}/digital-signature/{contractID}")]
        public ActionResult SignContract(SignContractVM vm)
        {
            FunctionResult validateResult = ValidateInput(vm);
            if (validateResult.Success == false)
            {
                return View(GetVMWithError(vm.LoginID, vm.ContractID, validateResult.ErrorMessage));
            }

            Contract contract = Contract.Get(vm.ContractID);

            if (!contract.IsSigned)
            {
                ContractDigitalSignature signature = new ContractDigitalSignature();
                signature.ContractID = vm.ContractID;
                signature.Date = vm.SignatureDate;
                signature.LoginID = vm.LoginID;
                signature.Name = vm.InputValue;
                signature.TimeStamp = DateTime.Now;

                ProEstimatorData.DataModel.SaveResult saveResult = signature.Insert();

                if (saveResult.Success)
                {
                    contract.IsSigned = true;
                    contract.Save();

                    ActiveLogin.HasUnsignedContract = false;

                    ContractManager.InsertSalesBoard(vm.LoginID, vm.ContractID);
                    DigitalSignaturePrintManager.CreateAndSendReport(signature);

                    return Redirect("/" + ActiveLogin.SiteUserID);
                }
                else
                {
                    return View(GetVMWithError(vm.LoginID, vm.ContractID, saveResult.ErrorMessage));
                }
            }
            else
            {
                return View(GetVMWithError(vm.LoginID, vm.ContractID, Proestimator.Resources.ProStrings.SignContract_AlreadySigned));
            }
        }

        private SignContractVM GetVMWithError(int loginID, int contractID, string errorMessage)
        {
            SignContractVMMapper mapper = new SignContractVMMapper();
            SignContractVM vm = mapper.Map(new SignContractVMMapperConfiguration() { LoginID = loginID, ContractID = contractID });

            vm.ErrorMessage = errorMessage;
            return vm;
        }

        private FunctionResult ValidateInput(SignContractVM vm)
        {
            if (string.IsNullOrEmpty(vm.InputValue) || vm.InputValue.Length < 3)
            {
                return new FunctionResult(Proestimator.Resources.ProStrings.SignContract_NoName);
            }

            int timezoneOffset = InputHelper.GetInteger(SiteSettings.Get(vm.LoginID, "TimeZone", "ReportOptions", "0").ValueString);
            DateTime currentDate = DateTime.Now.AddHours(timezoneOffset).Date;
            DateTime dateResult;
            int value;
            string sDate = vm.SignatureDate.Trim();
            if(sDate.Length == 8 && int.TryParse(sDate, out value))
            {
                sDate = sDate.Substring(0, 2) + "/" + sDate.Substring(2, 2) + "/" + sDate.Substring(4);
            }
            if (sDate.IndexOf("september", StringComparison.OrdinalIgnoreCase) == -1)
            {
                sDate = Regex.Replace(sDate, "sept", "Sep", RegexOptions.IgnoreCase);
            }

            if (ActiveLogin.LanguagePreference != "en-us")
            {
                if (DateTime.TryParse(sDate, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out dateResult) && dateResult.Date == currentDate)
                {
                    return new FunctionResult();
                }
            }

            if (!DateTime.TryParse(sDate, CultureInfo.CreateSpecificCulture(ActiveLogin.LanguagePreference), DateTimeStyles.None, out dateResult))
            {
                return new FunctionResult(Proestimator.Resources.ProStrings.SignContract_Date);
            }

            if (dateResult.Date != currentDate)
            {
                return new FunctionResult(Proestimator.Resources.ProStrings.EnterTodayDate);
            }

            return new FunctionResult();
        }

    }
}