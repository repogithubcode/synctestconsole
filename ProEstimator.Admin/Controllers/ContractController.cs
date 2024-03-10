using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using System.Text;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;


using ProEstimator.Admin.ViewModel.Contracts;
using Proestimator.Admin.ViewModelMappers.Contracts;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Logic;
using ProEstimator.Business.Payments;

namespace ProEstimator.Admin.Controllers
{
    public class ContractController : AdminController
    {
        private IPaymentService _paymentService;
        private IStripeService _stripeService;

        public ContractController(IPaymentService paymentService, IStripeService stripeService)
        {
            _paymentService = paymentService;
            _stripeService = stripeService;
        }

        [HttpGet]
        [Route("Contract/List/{loginID}")]
        public ActionResult Index(int loginID = 0)
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/Contract/List/" + loginID;
                return Redirect("/LogOut");
            }
            else
            {
                ContractPageVMMapper mapper = new ContractPageVMMapper();
                ContractPageVM vm = mapper.Map(new ContractPageVMMapperConfiguration() { LoginID = loginID });

                return View(vm);
            }
        }

        [HttpGet]
        [Route("Contract/List/Allsearch/{loginID}/{contractID}/{invoiceID}")]
        public ActionResult IndexAllSearch(int loginID = 0, int contractID = 0, int invoiceID = 0)
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/Contract/List/" + loginID;
                return Redirect("/LogOut");
            }
            else
            {
                ContractPageVMMapper mapper = new ContractPageVMMapper();
                ContractPageVM vm = mapper.Map(new ContractPageVMMapperConfiguration() { LoginID = loginID, ContractID = contractID,
                                                            InvoiceID = invoiceID
                });

                return View("Index", vm);
            }
        }

        [HttpGet]
        [Route("Contract/List/{loginID}/{contractID}/{invoiceID}")]
        public ActionResult Index(int loginID = 0, int contractID = 0, int invoiceID = 0)
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/Contract/List/" + loginID;
                return Redirect("/LogOut");
            }
            else
            {
                ContractPageVMMapper mapper = new ContractPageVMMapper();
                ContractPageVM vm = mapper.Map(new ContractPageVMMapperConfiguration() { LoginID = loginID });
                
                if(vm.GoodData)
                {
                    vm.ContractID = contractID;
                    vm.InvoiceID = invoiceID;
                }

                return View(vm);
            }
        }

        public JsonResult LoginSearch(string search)
        {
            int loginID = InputHelper.GetInteger(search);
            if (loginID > 0)
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                if (loginInfo != null)
                {
                    return Json(loginID, JsonRequestBehavior.AllowGet); 
                }
            }
            
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult Search(string searchLoginID, string searchContractID, string searchInvoiceID)
        {
            int loginID = InputHelper.GetInteger(searchLoginID);

            int contractID = InputHelper.GetInteger(searchContractID);

            int invoiceID = InputHelper.GetInteger(searchInvoiceID);

            // Login search
            if (loginID > 0)
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                if (loginInfo != null)
                {
                    return Json(loginID, JsonRequestBehavior.AllowGet);
                }
            }
            
            // Contract search
            if (contractID > 0)
            {
                Contract contractInfo = Contract.Get(contractID);
                if (contractInfo != null)
                {
                    return Json(contractInfo.LoginID, JsonRequestBehavior.AllowGet);
                }
            }

            // Invoice search
            if (invoiceID > 0)
            {
                Invoice invoiceInfo = Invoice.Get(invoiceID);
                if (invoiceInfo != null)
                {
                    return Json(invoiceInfo.LoginID, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        #region Trials

        public ActionResult GetTrialsForLogin([DataSourceRequest] DataSourceRequest request, int loginID, bool showDeleted = false)
        {
            List<TrialVM> trialVM = new List<TrialVM>();

            if (AdminIsValidated())
            {
                List<Trial> trials = Trial.GetForLogin(loginID, showDeleted);

                foreach (Trial trial in trials)
                {
                    trialVM.Add(new TrialVM(trial));
                }
            }

            return Json(trialVM.ToDataSourceResult(request));
        }

        public JsonResult CreateNewTrial(int loginID, string startDate, string endDate, bool ems, bool frameData, bool qbExporter, bool proAdvisor, bool images, bool customReports, bool bundle, bool multiUser)
        {
            if (AdminIsValidated())
            {
                TrialFunctionResult result = ContractManager.CreateTrial(loginID, InputHelper.GetDateTime(startDate), InputHelper.GetDateTime(endDate), ems, frameData, qbExporter, proAdvisor, images, customReports, bundle, multiUser);

                if (result.Success)
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(result.ErrorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("User is not validated", JsonRequestBehavior.AllowGet); 
            }
        }

        public JsonResult GetTrialDetails(int trialID)
        {
            if (AdminIsValidated())
            {
                Trial trial = Trial.Get(trialID);
                if (trial != null)
                {
                    TrialVM trialVM = new TrialVM(trial);
                    return Json(trialVM, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("User is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SaveTrialDetails(int trialID, string startDate, string endDate, bool isActive, bool isDeleted, bool hasEMS, bool hasFrameData, bool hasQBExporter, bool hasProAdvisor, bool hasImages, bool hasCustomReports, bool hasBundle, bool hasMultiUser)
        {
            if (AdminIsValidated())
            {
                Trial trial = Trial.Get(trialID);

                trial.StartDate = InputHelper.GetDateTime(startDate).Date;
                trial.EndDate = InputHelper.GetDateTime(endDate).Date;
                trial.Active = isActive;
                trial.IsDeleted = isDeleted;
                trial.HasEMS = hasEMS;
                trial.HasFrameData = hasFrameData;
                trial.HasQBExport = hasQBExporter;
                trial.HasProAdvisor = hasProAdvisor;
                trial.HasImages = hasImages;
                trial.HasCustomReports = hasCustomReports;
                trial.HasBundle = hasBundle;
                trial.HasMultiUser = hasMultiUser;

                SaveResult saveResult = trial.Save(GetSalesRepID());

                TrialVM trialVM = new TrialVM(trial);
                trialVM.ErrorMessage = saveResult.ErrorMessage;

                return Json(trialVM, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Contract Functions

        public ActionResult GetContractsForLogin([DataSourceRequest] DataSourceRequest request, int loginID, int contractID, int invoiceID, bool showDeleted = false)
        {
            List<ContractVM> contractVM = new List<ContractVM>();

            if (AdminIsValidated())
            {
                ContractVMMapper mapper = new ContractVMMapper();
                List<Contract> contracts = Contract.GetAllForLogin(loginID, showDeleted);

                if(contractID > 0)
                {
                    contracts = contracts.Where(eachContract => eachContract.ID == contractID).ToList();
                }

                foreach (Contract contract in contracts)
                {
                    contractVM.Add(mapper.Map(new ContractVMMapperConfiguration() { Contract = contract }));
                }
            }

            return Json(contractVM.ToDataSourceResult(request));
        }

        public ActionResult GetContractTermsOptions([DataSourceRequest] DataSourceRequest request, int contractID, int contractType, int priceLevel, int qty)
        {
            List<ContractTermsVM> result = new List<ContractTermsVM>();

            if (AdminIsValidated())
            {
                try
                {
                    List<ContractTerms> allContractTerms = ContractManager.GetContractTermsForAddOn(contractID, contractType, true);
                    foreach (ContractTerms contractTerms in allContractTerms)
                    {
                        ContractPriceLevel contractPriceLevel = ContractPriceLevel.GetAll().FirstOrDefault(o => o.Active && (o.PriceLevel == priceLevel || contractType != 1) && o.ContractTerms.ID == contractTerms.ID && o.PaymentAmount > 0);

                        if (contractPriceLevel != null)
                        {
                            ContractTermsVM vm = new ContractTermsVM();
                            vm.ContractPriceLevelID = contractPriceLevel.ID;
                            vm.DepositAmount = qty * contractTerms.DepositAmount;
                            vm.ForceAutoPay = contractTerms.ForceAutoPay;
                            vm.NumberOfPayments = contractTerms.NumberOfPayments;
                            vm.PaymentDescription = (contractTerms.DepositAmount > 0 ? "Deposit of " + contractTerms.DepositAmount.ToString("C") + " then " : "") + contractTerms.NumberOfPayments.ToString() + " payment" + (contractTerms.NumberOfPayments > 0 ? "s" : "") + " of " + contractPriceLevel.PaymentAmount.ToString("C");
                            vm.TermDescription = contractTerms.TermDescription;
                            vm.PaymentAmount = contractPriceLevel.PaymentAmount;
                            vm.TermTotal = qty * contractTerms.DepositAmount + (qty * contractTerms.NumberOfPayments * contractPriceLevel.PaymentAmount);
                            string qtyString = "";
                            if (qty > 1)
                            {
                                qtyString = qty.ToString() + " x ";
                            }
                            vm.Summary = (contractTerms.DepositAmount > 0 ? qtyString + contractTerms.DepositAmount.ToString("C") + " + " : "") + qtyString + contractTerms.NumberOfPayments.ToString() + " x " + contractPriceLevel.PaymentAmount.ToString("C");
                            
                            result.Add(vm);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, 0, "GetContractTersmDetailsForContractType");
                }
            }

            return Json(result.ToDataSourceResult(request));
        }

        public JsonResult CreateNewContract(int loginID, int contractPriceLevelID, string startDate)
        {
            if (AdminIsValidated())
            {
                StringBuilder validationBuilder = new StringBuilder();

                // Make sure a valid start date was entered, don't default to today
                DateTime startDateTime = InputHelper.GetDateTime(startDate, DateTime.MinValue);
                if (startDateTime.Year < DateTime.Now.AddYears(-100).Year || startDateTime.Year > DateTime.Now.AddYears(100).Year)
                {
                    validationBuilder.AppendLine("Enter a valid start date.");
                }

                if (contractPriceLevelID == 0)
                {
                    validationBuilder.AppendLine("Select a line from the Terms Description grid.");
                }

                if (validationBuilder.Length > 0)
                {
                    return Json(validationBuilder.ToString(), JsonRequestBehavior.AllowGet); 
                }

                ContractFunctionResult result = ContractManager.CreateContract(loginID, contractPriceLevelID, startDate);

                if (result.Success)
                {
                    if (ContractManager.IsInEarlyRenewalPeriod(loginID, ContractManager.MaxRenewalWindow))
                    {
                        try
                        {
                            result.Contract.EarlyRenewal = true;
                            result.Contract.Save();
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError(ex, loginID, "Admin CreateNewContract SetEarlyRenewal");
                        }
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(result.ErrorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("User is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        private void RefreshFrontEndContractInfo(int loginID)
        {
            FrontEndCommunicator communicator = new FrontEndCommunicator();
            communicator.RefreshContractsOnSite(loginID);
        }

        public JsonResult GetContractDetails(int contractID)
        {
            if (AdminIsValidated())
            {
                Contract contract = Contract.Get(contractID);
                if (contract != null)
                {
                    ContractVMMapper mapper = new ContractVMMapper();
                    ContractVM contractVM = mapper.Map(new ContractVMMapperConfiguration() { Contract = contract, IncludeAvailablePromoCodes = true });

                    // The Digital Signature ID is not in the contract table, get it now and add it to the vm
                    ContractDigitalSignature digitalSignature = ContractDigitalSignature.GetForContract(contractID);
                    if (digitalSignature != null)
                    {
                        string diskPath = DigitalSignaturePrintManager.GetDiskPath(digitalSignature);
                        if (System.IO.File.Exists(diskPath))
                        {
                            contractVM.DigitalSignatureID = digitalSignature.ID; ;
                        }
                    }

                    return Json(contractVM, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SaveContractDetails(int contractID, string notes, string startDate, string endDate, bool isSigned, bool isActive, bool ignoreAutoPay, bool earlyRenewal, bool isDeleted)
        {
            if (AdminIsValidated())
            {
                Contract contract = Contract.Get(contractID);

                contract.Notes = notes;
                contract.EffectiveDate = InputHelper.GetDateTime(startDate).Date;
                contract.ExpirationDate = InputHelper.GetDateTime(endDate).Date;
                contract.IsSigned = isSigned;
                contract.IgnoreAutoPay = ignoreAutoPay;
                contract.EarlyRenewal = earlyRenewal;
                contract.Active = isActive;
                contract.IsDeleted = isDeleted;

                SaveResult saveResult = contract.Save(ActiveLogin.ID);

                ContractVMMapper mapper = new ContractVMMapper();
                ContractVM contractVM = mapper.Map(new ContractVMMapperConfiguration() { Contract = contract, IncludeAvailablePromoCodes = true });
                contractVM.ErrorMessage = saveResult.ErrorMessage;

                //if (saveResult.Success && contractID > 0)
                //{
                //    SuccessBoxDataSyncer.SuccessBoxContractLogUpdate(contractID, 0, false);
                //}

                RefreshFrontEndContractInfo(contract.LoginID);

                return Json(contractVM, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("User is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ApplyPromoCode(int contractID, string promoCode)
        {
            string errorMessage = "";

            if (AdminIsValidated())
            {
                Contract contract = Contract.Get(contractID);
                if (contract != null)
                {
                    FunctionResult result = ContractManager.ApplyPromo(contract, promoCode, ActiveLogin.ID, true);
                    if (!result.Success)
                    {
                        errorMessage = result.ErrorMessage;
                    }
                }
                else
                {
                    errorMessage = "Error: No in progress contract found.";
                }
            }

            return Json(errorMessage, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RemovePromo(int contractID)
        {
            if (AdminIsValidated())
            {
                Contract contract = Contract.Get(contractID);

                FunctionResult result = ContractManager.RemovePromo(contract, ActiveLogin.ID);
                if (!result.Success)
                {
                    return Json(result.ErrorMessage, JsonRequestBehavior.AllowGet);
                }

                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ViewContract(int id)
        {
            ContractDigitalSignature digitalSignature = ContractDigitalSignature.Get(id);
            if (digitalSignature != null)
            {
                string diskPath = DigitalSignaturePrintManager.GetDiskPath(digitalSignature);

                if (!DigitalSignaturePrintManager.DoesPrintExist(digitalSignature))
                {
                    FunctionResult result = DigitalSignaturePrintManager.CreateReport(digitalSignature);
                    if (!result.Success)
                    {
                        return Content("Error: " + result.ErrorMessage);
                    }
                }

                if (!string.IsNullOrEmpty(diskPath) && System.IO.File.Exists(diskPath))
                {
                    var fileStream = new FileStream(diskPath, FileMode.Open, FileAccess.Read);
                    var fsResult = new FileStreamResult(fileStream, "application/pdf");
                    return fsResult;
                }
            }

            //filename from querystring or estimate id from session is not valid...show the error message instead of blank screen.
            return Content("Something went wrong..! Please  open/generate report again.");
        }

        #endregion

        #region Add Ons

        public ActionResult GetAddOnsForContract([DataSourceRequest] DataSourceRequest request, int contractID, bool showDeleted = false)
        {
            List<AddOnVM> addOnVMs = new List<AddOnVM>();

            if (AdminIsValidated())
            {
                Contract contract = Contract.Get(contractID);

                if (contract != null)
                {
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(contractID, showDeleted);

                    AddOnVMMapper mapper = new AddOnVMMapper();
                    foreach (ContractAddOn addOn in addOns)
                    {
                        addOnVMs.Add(mapper.Map(new AddOnVMMapperConfiguration() { AddOn = addOn }));
                    }
                }
            }

            return Json(addOnVMs.ToDataSourceResult(request));
        }

        public JsonResult HasAddOn(int contractID, int addOnType)
        {
            if (AdminIsValidated())
            {
                return Json(new FunctionResult(ContractAddOn.GetForContract(contractID).FirstOrDefault(o => o.AddOnType.ID == addOnType) != null, ""), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new FunctionResult("Admin is not validated for Refresh [Create New Add On] Button"), JsonRequestBehavior.AllowGet);
            }
        }
        
        public JsonResult CreateNewAddOn(int contractID, int addOnType, int contractPriceLevelID, string startDate, int qty)
        {
            if (AdminIsValidated())
            {
                Contract contract = Contract.Get(contractID);

                FunctionResult result = ContractManager.CreateContractAddOn(contract, contractPriceLevelID, addOnType, InputHelper.GetDateTime(startDate), qty);

                if (result.Success)
                {
                    // If this is a bundle add on, delete any existing bundlable addons
                    if (addOnType == 12)
                    {
                        List<ContractAddOn> addOns = ContractAddOn.GetForContract(contractID);
                        foreach(ContractAddOn addOn in addOns)
                        {
                            if (addOn.AddOnType.IsBundlable)
                            {
                                addOn.IsDeleted = true;
                                addOn.Save(ActiveLogin.ID);
                            }
                        }
                    }

                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(result.ErrorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAddOnDetails(int addonID)
        {
            if (AdminIsValidated())
            {
                ContractAddOn addOn = ContractAddOn.Get(addonID);
                if (addOn != null)
                {
                    AddOnVMMapper mapper = new AddOnVMMapper();
                    AddOnVM addonVM = mapper.Map(new AddOnVMMapperConfiguration() { AddOn = addOn });
                    
                    return Json(addonVM, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SaveAddOnDetails(int addonID, bool active, bool isDeleted, string startDate, int qty)
        {
            if (AdminIsValidated())
            {
                ContractAddOn addOn = ContractAddOn.Get(addonID);

                addOn.Active = active;
                addOn.IsDeleted = isDeleted;
                addOn.StartDate = InputHelper.GetDateTime(startDate).Date;
                
                SaveResult saveResult = addOn.Save(ActiveLogin.ID);

                int addQuantity = qty - addOn.Quantity;
                if (addQuantity > 0)
                {
                    JsonResult json = CreateNewAddOn(addOn.ContractID, addOn.AddOnType.ID, addOn.PriceLevel.ID, DateTime.Now.ToShortDateString(), addQuantity);
                    string error = Convert.ToString(json.Data);
                    if (error != "")
                    {
                        ErrorLogger.LogError("AddOn ID: " + addOn.ID + " Error: " + error, "Admin CreateNewAddOn SaveAddOnDetails");
                    }
                }
                else if(addQuantity < 0)
                {
                    FunctionResult result = ContractManager.RemoveContractAddOn(0, addOn.ID, addQuantity * -1, ActiveLogin.ID);
                    if (result.Success == false)
                    {
                        ErrorLogger.LogError("AddOn ID: " + addOn.ID + " Error: " + result.ErrorMessage, "Admin RemoveContractAddOn SaveAddOnDetails");
                    }
                }

                AddOnVMMapper mapper = new AddOnVMMapper();
                AddOnVM addonVM = mapper.Map(new AddOnVMMapperConfiguration() { AddOn = ContractAddOn.Get(addonID) });
                addonVM.ErrorMessage = saveResult.ErrorMessage;

                Contract contract = Contract.Get(addOn.ContractID);
                RefreshFrontEndContractInfo(contract.LoginID);

                return Json(addonVM, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Add On Trials

        public ActionResult GetAddOnTrialsForContract([DataSourceRequest] DataSourceRequest request, int contractID, bool showDeleted = false)
        {
            List<AddOnTrialVM> addOnTrialVMs = new List<AddOnTrialVM>();

            if (AdminIsValidated())
            {
                Contract contract = Contract.Get(contractID);

                if (contract != null)
                {
                    List<ContractAddOnTrial> addOnTrials = ContractAddOnTrial.GetForContract(contractID, showDeleted);

                    foreach (ContractAddOnTrial addOnTrial in addOnTrials)
                    {
                        addOnTrialVMs.Add(new AddOnTrialVM(addOnTrial));
                    }
                }
            }

            return Json(addOnTrialVMs.ToDataSourceResult(request));
        }

        public JsonResult CreateNewAddOnTrial(int contractID, int addOnType, string startDate, string endDate)
        {
            if (AdminIsValidated())
            {
                Contract contract = Contract.Get(contractID);

                FunctionResult result = ContractManager.CreateContractAddOnTrial(contract, addOnType, InputHelper.GetDateTime(startDate), InputHelper.GetDateTime(endDate));

                if (result.Success)
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(result.ErrorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAddOnTrialDetails(int addonTrialID)
        {
            if (AdminIsValidated())
            {
                ContractAddOnTrial addOnTrial = ContractAddOnTrial.Get(addonTrialID);
                if (addOnTrial != null)
                {
                    AddOnTrialVM addonTrialVM = new AddOnTrialVM(addOnTrial);
                    return Json(addonTrialVM, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SaveAddOnTrialDetails(int addonTrialID, bool isDeleted, string startDate, string endDate)
        {
            if (AdminIsValidated())
            {
                ContractAddOnTrial addOnTrial = ContractAddOnTrial.Get(addonTrialID);

                addOnTrial.IsDeleted = isDeleted;
                addOnTrial.StartDate = InputHelper.GetDateTime(startDate).Date;
                addOnTrial.EndDate = InputHelper.GetDateTime(endDate).Date;

                SaveResult saveResult = addOnTrial.Save(ActiveLogin.ID);

                AddOnTrialVM addonTrialVM = new AddOnTrialVM(addOnTrial);
                addonTrialVM.ErrorMessage = saveResult.ErrorMessage;

                Contract contract = Contract.Get(addOnTrial.ContractID);
                RefreshFrontEndContractInfo(contract.LoginID);

                return Json(addonTrialVM, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Invoices

        public ActionResult GetInvoiceFailureLog([DataSourceRequest] DataSourceRequest request, int invoiceID)
        {
            List<InvoiceFailureLogGridVM> returnList = new List<InvoiceFailureLogGridVM>();

            List<InvoiceFailureLog> failureLogs = InvoiceFailureLog.GetForInvoiceAdmin(invoiceID);

            InvoiceFailureLogGridVMMapper mapper = new InvoiceFailureLogGridVMMapper();
            foreach (InvoiceFailureLog failureLog in failureLogs)
            {
                returnList.Add(mapper.Map(new InvoiceFailureLogGridVMMapperConfiguration() { FailureLog = failureLog }));
            }

            return Json(returnList.ToDataSourceResult(request));
        }

        public ActionResult GetInvoiceHistory([DataSourceRequest] DataSourceRequest request, int invoiceID)
        {
            List<InvoiceHistoryVM> returnList = new List<InvoiceHistoryVM>();

            List<ContractAddOnHistory> historys = ContractAddOnHistory.GetForInvoice(invoiceID);

            InvoiceHistoryVMMapper mapper = new InvoiceHistoryVMMapper();
            foreach (ContractAddOnHistory history in historys)
            {
                returnList.Add(mapper.Map(new InvoiceHistoryVMMapperConfiguration() { History = history }));
            }

            return Json(returnList.ToDataSourceResult(request));
        }

        private List<InvoiceGridVM> GetInvoices(int contractID, bool showDeleted, int typeID, bool showAddOnDeleted)
        {
            List<InvoiceGridVM> invoiceList = new List<InvoiceGridVM>();

            if (AdminIsValidated() && contractID > 0)
            {
                Contract contract = Contract.Get(contractID);

                if (contract != null)
                {
                    if (typeID == 0)
                    {
                        List<Invoice> baseInvoices = Invoice.GetForContractForAdmin(contract.ID, false, showDeleted).Where(o => o.InvoiceType.ID != 3).ToList();
                        foreach (Invoice invoice in baseInvoices.OrderBy(o => o.DueDate))
                        {
                            invoiceList.Add(new InvoiceGridVM(invoice));
                        }
                    }
                    else
                    {
                        List<Invoice> invoices = Invoice.GetForContractForAdmin(contract.ID, true, showDeleted, showAddOnDeleted).Where(o => o.InvoiceType.ID != 3).ToList();  // Invoice Type 3 is Custom, those are showing in their own grid

                        // Get the contract add on based on the type ID
                        List<ContractAddOn> addOns = ContractAddOn.GetForContract(contractID, showAddOnDeleted).Where(o => o.AddOnType.ID == typeID).ToList();
                        foreach (ContractAddOn addOn in addOns)
                        {
                            foreach (Invoice invoice in invoices.Where(o => o.AddOnID == addOn.ID).OrderBy(o => o.AddOnID).ThenBy(o => o.DueDate))
                            {
                                invoiceList.Add(new InvoiceGridVM(invoice));
                            }
                        }
                    }
                }
            }

            return invoiceList;
        }

        public JsonResult GetContractGridsList(int contractID, bool showDeleted = false, bool showAddOnDeleted = false, bool showAddOnTrialDeleted = false, bool addon = true, bool addonTrial = true, bool invoice = true)
        {
            if (AdminIsValidated())
            {
                ContractGridsListVMMapper mapper = new ContractGridsListVMMapper();
                ContractGridsListVM contractGridsVM = mapper.Map(new ContractGridsListVMMapperConfiguration() { ContractID = contractID, ShowDeleted = showDeleted, ShowAddOnDeleted = showAddOnDeleted, ShowAddOnTrialDeleted = showAddOnTrialDeleted, AddOn = addon, AddOnTrial = addonTrial, Invoice = invoice });

                if (contractGridsVM != null)
                {
                    return Json(contractGridsVM, JsonRequestBehavior.AllowGet);
                }
            }
            
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetInvoiceDetails(int invoiceID)
        {
            if (AdminIsValidated())
            {
                Invoice invoice = Invoice.Get(invoiceID);
                if (invoice != null)
                {
                    InvoiceVM invoiceVM = new InvoiceVM(invoice);
                    SetupInvoiceVMLinks(invoiceVM, invoice);                    

                    return Json(invoiceVM, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        private void SetupInvoiceVMLinks(InvoiceVM invoiceVM, Invoice invoice)
        {
            invoiceVM.Links = new List<InvoiceLinkVM>();
            invoiceVM.Links.Add(new InvoiceLinkVM(0, "Contract"));

            List<ContractAddOn> addOns = ContractAddOn.GetForContract(invoice.ContractID);
            foreach (ContractAddOn addOn in addOns)
            {
                invoiceVM.Links.Add(new InvoiceLinkVM(addOn.ID, addOn.AddOnType.Type + "(" + addOn.PriceLevel.ContractTerms.TermDescription + ")"));
            }

            invoiceVM.InvoiceLinkID = invoice.AddOnID;
        }

        public JsonResult SaveInvoiceDetails(int invoiceID, int linkID, string amount, string tax, string dueDate, string notes, string summary, bool isPaid, string paidDate, bool isDeleted)
        {
            if (AdminIsValidated())
            {
                Invoice invoice = Invoice.Get(invoiceID);
                if (invoice != null)
                {
                    bool markedPaid = false;

                    if (!invoice.Paid && isPaid)
                    {
                        markedPaid = true;
                    }

                    invoice.InvoiceAmount = InputHelper.GetDecimal(amount);
                    invoice.AddOnID = linkID;
                    invoice.SalesTax = InputHelper.GetDecimal(tax);
                    invoice.DueDate = InputHelper.GetDateTime(dueDate).Date;
                    invoice.Notes = notes;
                    invoice.Summary = summary;
                    invoice.Paid = isPaid;
                    invoice.DatePaid = InputHelper.GetDateTime(paidDate).Date;
                    invoice.IsDeleted = isDeleted;

                    SaveResult saveResult = invoice.Save(ActiveLogin.ID);

                    if (markedPaid)
                    {
                        _paymentService.MarkInvoicePaid(invoice, invoice.PaymentID, true);
                    }

                    InvoiceVM invoiceVM = new InvoiceVM(invoice);
                    SetupInvoiceVMLinks(invoiceVM, invoice);
                    invoiceVM.ErrorMessage = saveResult.ErrorMessage;

                    //if (saveResult.Success && invoiceID > 0)
                    //{
                    //    SuccessBoxDataSyncer.SuccessBoxInvoiceLogUpdate(invoiceID, false);
                    //}

                    RefreshFrontEndContractInfo(invoice.LoginID);

                    return Json(invoiceVM, JsonRequestBehavior.AllowGet);
                }

                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GridLineSaveInvoiceDetails(int invoiceID, string salesTax, int linkID)
        {
            if (AdminIsValidated())
            {
                Invoice invoice = Invoice.Get(invoiceID);
                if (invoice != null)
                {
                    invoice.AddOnID = linkID;
                    if (salesTax.Contains("$") && salesTax.IndexOf("$") == 0)
                    {
                        salesTax = salesTax.Substring(1);
                    }

                    invoice.SalesTax = InputHelper.GetDecimal(salesTax);

                    SaveResult saveResult = invoice.Save(ActiveLogin.ID);

                    //if (markedPaid)
                    //{
                    //    ProEstimator.Business.Payments.PaymentHelper paymentHelper = new Business.Payments.PaymentHelper();
                    //    paymentHelper.MarkInvoicePaid(invoice, invoice.PaymentID, true);
                    //}

                    InvoiceVM invoiceVM = new InvoiceVM(invoice);
                    SetupInvoiceVMLinks(invoiceVM, invoice);
                    invoiceVM.ErrorMessage = saveResult.ErrorMessage;

                    //if (saveResult.Success && invoiceID > 0)
                    //{
                    //    SuccessBoxDataSyncer.SuccessBoxInvoiceLogUpdate(invoiceID, false);
                    //}

                    return Json(invoiceVM, JsonRequestBehavior.AllowGet);
                }

                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CalculateInvoiceTax(int invoiceID, int loginID, string amount)
        {
            if (AdminIsValidated())
            {
                decimal amountDecimal = InputHelper.GetDecimal(amount, -1);
                
                if (amountDecimal == -1)
                {
                    return Json(new TaxFunctionResult("Invalid invoice amount."), JsonRequestBehavior.AllowGet);
                }

                try
                {
                    Invoice invoice = Invoice.Get(invoiceID);
                    invoice.InvoiceAmount = amountDecimal;

                    if (invoice.LoginID != loginID)
                    {
                        return Json(new TaxFunctionResult("The invoice does not belong to the login."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        FunctionResult functionResult = TaxManager.CalculateTaxForInvoice(invoice, Address.GetForLoginID(loginID));
                        if (functionResult.Success)
                        {
                            return Json(new TaxFunctionResult(amountDecimal, invoice.SalesTax), JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new TaxFunctionResult(functionResult.ErrorMessage), JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new TaxFunctionResult("Error: " + ex.Message), JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GridLineCalculateInvoiceTax(string invoiceIDs, int loginID)
        {
            string errorMessage = string.Empty;

            if (AdminIsValidated())
            {
                try
                {
                    string[] splitStrArr = new string[1];
                    splitStrArr[0] = ",";

                    string[] invoiceArr = invoiceIDs.Split(splitStrArr, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string eachInvoiceID in invoiceArr)
                    {
                        int invoiceID = InputHelper.GetInteger(eachInvoiceID);

                        Invoice invoice = Invoice.Get(invoiceID);

                        if (invoice.LoginID != loginID)
                        {
                            return Json(new TaxFunctionResult("The invoice does not belong to the login."), JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            FunctionResult functionResult = TaxManager.CalculateTaxForInvoice(invoice, Address.GetForLoginID(loginID));
                            if (!functionResult.Success)
                            {
                                errorMessage = functionResult.ErrorMessage;
                                return Json(new TaxFunctionResult(errorMessage), JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    return Json(new TaxFunctionResult(errorMessage), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new TaxFunctionResult("Error: " + ex.Message), JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        public class TaxFunctionResult : FunctionResult
        {
            public decimal Amount { get; set; }
            public decimal Tax { get; set; }

            public TaxFunctionResult(decimal amount, decimal tax)
                : base()
            {
                Amount = amount;
                Tax = tax;
            }

            public TaxFunctionResult(string error)
                : base(error)
            {

            }
        }

        public JsonResult FillCustomInvoiceLinkedToDLL(int contractID)
        {
            List<AddOnLinkData> data = new List<AddOnLinkData>();

            if (AdminIsValidated())
            {
                List<ContractAddOn> addOns = ContractAddOn.GetForContract(contractID);
                if (addOns.Count > 0)
                {
                    data.Add(new AddOnLinkData(0, "Contract"));

                    foreach(ContractAddOn addOn in addOns)
                    {
                        data.Add(new AddOnLinkData(addOn.ID, addOn.AddOnType.Type + "(" + addOn.PriceLevel.ContractTerms.TermDescription + ")"));
                    }
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet); 
        }

        public JsonResult BatchInvoicesPaid(string ids, Boolean paid)
        {
            string[] pieces = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            int counter = 0;

            foreach (string piece in pieces)
            {
                int invoiceID = InputHelper.GetInteger(piece);
                if (invoiceID > 0)
                {
                    Invoice invoice = Invoice.Get(invoiceID);
                    if (invoice != null)
                    {
                        if(paid==true)
                        {
                            _paymentService.MarkInvoicePaid(invoice, 0, true);
                        }
                        else
                        {
                            _paymentService.MarkInvoiceUnPaid(invoice, 0, true);
                        }
                        counter++;

                        RefreshFrontEndContractInfo(invoice.LoginID);
                    }
                }
            }

            return Json(counter + " invoice(s) set to paid", JsonRequestBehavior.AllowGet); 
        }

        public JsonResult BatchInvoicesDelete(string ids)
        {
            string[] pieces = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            int counter = 0;

            foreach (string piece in pieces)
            {
                int invoiceID = InputHelper.GetInteger(piece);
                if (invoiceID > 0)
                {
                    Invoice invoice = Invoice.Get(invoiceID);
                    if (invoice != null && !invoice.IsDeleted)
                    {
                        invoice.IsDeleted = true;
                        invoice.Save(ActiveLogin.ID);
                        counter++;
                    }
                }
            }

            return Json(counter + " invoice(s) deleted", JsonRequestBehavior.AllowGet);
        }

        public class AddOnLinkData
        {
            public int ID { get; set; }
            public string Name { get; set; }

            public AddOnLinkData() { }

            public AddOnLinkData(int id, string name)
            {
                ID = id;
                Name = name;
            }
        }

        public JsonResult CreateCustomInvoices(int loginID, int contractID, int addOnID, string startDate, int monthSkip, string invoiceAmount, string numberOfInvoices, string notes)
        {
            if (AdminIsValidated())
            {
                Contract contract = Contract.Get(contractID);
                if (contract == null || contract.LoginID != loginID)
                {
                    return Json("Error: Invalid contract.", JsonRequestBehavior.AllowGet);
                }

                DateTime startDateTime = InputHelper.GetDateTime(startDate);
                decimal invoiceAmountDollars = InputHelper.GetDecimal(invoiceAmount);
                int numberOfInvoicesInt = InputHelper.GetInteger(numberOfInvoices);

                if (invoiceAmountDollars <= 0)
                {
                    return Json("Error: Invalid invoice amount.", JsonRequestBehavior.AllowGet);
                }

                if (numberOfInvoicesInt <= 0)
                {
                    return Json("Error: Number of invoices must be at least 1.", JsonRequestBehavior.AllowGet);
                }

                // Inputs are good, create the invoices
                Address address = Address.GetForLoginID(loginID);

                ContractAddOn contractAddOn = null;
                if (addOnID > 0)
                {
                    contractAddOn = ContractAddOn.Get(addOnID);
                }

                try
                {
                    int monthAdd = 0;
                    for (int i = 0; i < numberOfInvoicesInt; i++)
                    {
                        ContractManager.CreateInvoice(loginID, contract, 0, 3, invoiceAmountDollars, startDateTime.AddMonths(monthAdd), address, contractAddOn, notes);
                        monthAdd += monthSkip;
                    }
                }
                catch (Exception ex)
                {
                    return Json("Error: " + ex.Message, JsonRequestBehavior.AllowGet);
                }

                return Json("", JsonRequestBehavior.AllowGet); 
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
   
}