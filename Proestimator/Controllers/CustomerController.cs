using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using ProEstimator.Business.Logic;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using Proestimator.ViewModel;
using Proestimator.ViewModel.Customer;
using System.Configuration;
using ProEstimator.Business.ProAdvisor;

namespace Proestimator.Controllers
{
    public class CustomerController : SiteController
    {
        private IProAdvisorService _proAdvisorService;

        public CustomerController(IProAdvisorService proAdvisorService)
        {
            _proAdvisorService = proAdvisorService;
        }

        #region Customer Search
              

        [HttpGet]
        [Route("{userID}")]
        public ActionResult CustomerSearch(int userID, bool deleted = false)
        {
            CustomerSearchVM vm = new CustomerSearchVM();
            vm.UserID = userID;
            vm.State = "";
            vm.States = GetStates().ToList();
            vm.ConversionComplete = LoginInfo.IsWebEstConversionComplete(ActiveLogin.LoginID);

            ViewBag.HomePageMessage = SiteGlobalsManager.HomePageMessage.Replace("\r\n", "<br />");

            // Check for due or soon to be due invoices
            try
            {
                List<Invoice> invoices = ContractManager.GetInvoicesToBePaid(ActiveLogin.LoginID);
                if (invoices.Count > 0)
                {
                    DateTime nextDueDate = GetNextDueDate(invoices);
                    int daysUntilDue = (nextDueDate - DateTime.Now).Days;

                    if (daysUntilDue <= 14)
                    {
                        decimal totalDue = GetTotalDueForDate(invoices, nextDueDate);

                        StripeInfo stripeInfo = StripeInfo.GetForLogin(ActiveLogin.LoginID);

                        if (stripeInfo == null || stripeInfo.AutoPay == false)      // Only show up coming invoice reminders if auto pay is off
                        {
                            if (daysUntilDue > 0)
                            {
                                vm.InvoiceReminder = String.Format(@Proestimator.Resources.ProStrings.InvoiceReminder_DueOnDate, totalDue.ToString("C2"), DateTime.Now.AddDays(daysUntilDue).ToShortDateString());
                            }
                            if (daysUntilDue == 0)
                            {
                                vm.InvoiceReminder = String.Format(@Proestimator.Resources.ProStrings.InvoiceReminder_DueToday, totalDue.ToString("C2"));
                            }
                        }
                        else if (stripeInfo != null && stripeInfo.AutoPay == true)
                        {
                            if (daysUntilDue <= GetAutoPayWindow() && daysUntilDue > 0)
                            {
                                vm.InvoiceReminder = String.Format(@Proestimator.Resources.ProStrings.InvoiceReminder_AutoPay, totalDue.ToString("C2"), daysUntilDue.ToString());
                            }
                        }

                        if (daysUntilDue < 0)
                        {
                            vm.InvoiceReminder = String.Format(@Proestimator.Resources.ProStrings.InvoiceReminder_Overdue, totalDue.ToString("C2"));
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, ActiveLogin.LoginID, "CustomerSearch CheckInvoices");
            }

            //if (ActiveLogin.HasProAdvisorContract)
            //{
                double addOnTotal = _proAdvisorService.GetAddOnTotalForLogin(ActiveLogin.LoginID);
                if (addOnTotal > 0)
                {
                    ViewBag.AddOnMessage = "ProAdvisor suggestions selected " + addOnTotal.ToString("C");
                }
            //}

            vm.ContractReminder = ActiveLogin.ContractReminder;
            vm.ShowEarlyRenewal = ActiveLogin.ShowEarlyRenewal;            

            ViewBag.NavID = "estimate";

            return View(vm);
        }

        private DateTime GetNextDueDate(List<Invoice> invoices)
        {
            if (invoices == null || invoices.Count == 0)
            {
                return DateTime.Now;
            }

            invoices = invoices.OrderBy(o => o.DueDate).ToList();
            DateTime nextDueDate = invoices.First().DueDate;

            if (nextDueDate <= DateTime.Now)
            {
                nextDueDate = DateTime.Now;
            }

            return nextDueDate;
        }

        private Decimal GetTotalDueForDate(List<Invoice> invoices, DateTime date)
        {
            Decimal total = 0;

            foreach (Invoice invoice in invoices)
            {
                if (invoice.DueDate.Date <= date.Date)
                {
                    total += invoice.InvoiceTotal;
                }
            }

            return total;
        }

        private int GetAutoPayWindow()
        {
            int window = 3;

            try
            {
                if (ConfigurationManager.AppSettings["AutoPayMessageWindow"] != null)
                {
                    window = InputHelper.GetInteger(ConfigurationManager.AppSettings["AutoPayMessageWindow"]);
                }
            }
            catch { }

            return window;
        }

        [HttpPost]
        [Route("{userID}")]
        public ActionResult CustomerSearch(CustomerSearchVM vm)
        {
            return DoRedirect("/");
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/customer-selection")]
        public PartialViewResult CustomerSearchForSelection(int userID, int estimateID)
        {
            CustomerSearchVM vm = new CustomerSearchVM();
            vm.UserID = userID;
            vm.EstimateID = estimateID;
            vm.State = "";
            vm.States = GetStates().ToList();
            vm.ForCustomerSelection = true;

            ViewBag.NavID = "customersearch";
            ViewBag.EstimateID = estimateID;

            return PartialView("_CustomerGrid", vm);
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/select-customer/{customerID}")]
        public ActionResult CustomerSearchForSelection(int userID, int estimateID, int customerID)
        {
            CacheActiveLoginID(userID);

            Estimate estimate = new Estimate(estimateID);
            if (estimate != null && estimate.CreatedByLoginID == ActiveLogin.LoginID)
            {
                Customer customer = Customer.Get(customerID);
                if (customer != null && customer.LoginID == ActiveLogin.LoginID)
                {
                    estimate.CustomerID = customer.ID;
                    SaveResult saveResult = estimate.Save(ActiveLogin.ID);

                    if (saveResult.Success)
                    {
                        return Redirect("/" + userID + "/estimate/" + estimateID + "/customer");
                    }
                    else
                    {
                        return View(saveResult.ErrorMessage);
                    }
                }
            }

            return View(@Proestimator.Resources.ProStrings.InvalidData);
        }

        public ActionResult GetCustomerSearch(
              [DataSourceRequest] DataSourceRequest request
            , int userID
            , string searchText
            , bool showDeleted
        )
        {
            CacheActiveLoginID(userID);

            List<CustomerSearchResultVM> customerList = new List<CustomerSearchResultVM>();

            if (IsUserAuthorized(userID))
            {
                try
                {
                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                    List<Customer> customers = Customer.GetForLogin(activeLogin.LoginID).Where(o => o.IsDeleted == showDeleted).ToList();

                    if (string.IsNullOrEmpty(searchText))
                    {
                        foreach (Customer customer in customers)
                        {
                            customerList.Add(new CustomerSearchResultVM(customer, new List<string>()));
                        }
                    }
                    else
                    {
                        List<string> words = searchText.ToLower().Split(" ".ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                        foreach (Customer customer in customers)
                        {
                            bool matched = true;

                            foreach (string word in words)
                            {
                                if (!(customer.Contact.FirstName.ToLower().Contains(word)
                                    || customer.Contact.LastName.ToLower().Contains(word)
                                    || customer.Contact.Email.ToLower().Contains(word)
                                    || customer.Contact.Phone.ToLower().Contains(word)
                                    || customer.Contact.BusinessName.ToLower().Contains(word)
                                    || customer.Address.Line1.ToLower().Contains(word)
                                    || customer.Address.Zip.ToLower().Contains(word))
                                )
                                {
                                    matched = false;
                                }
                            }

                            if (matched)
                            {
                                customerList.Add(new CustomerSearchResultVM(customer, words));
                            }
                        }

                        try
                        {
                            SuccessBoxFeatureLog.LogFeature(activeLogin.LoginID, SuccessBoxModule.Search, "Doing a customer search", activeLogin.ID);
                        }
                        catch(Exception ex)
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, ActiveLogin.LoginID, "GetCustomerSEearch");
                }
            }

            return Json(customerList.ToDataSourceResult(request));
        }

        public JsonResult GetCustomerDetails(int userID, int customerID)
        {
            CustomerDetailsVM vm = new CustomerDetailsVM();

            // Make sure the passed LoginID is ok
            if (IsUserAuthorized(userID))
            {
                CacheActiveLoginID(userID);

                Customer customer = Customer.Get(customerID);
                if (customer != null && customer.LoginID == _siteLoginManager.GetActiveLogin(userID, GetComputerKey()).LoginID)
                {
                    vm.Success = true;

                    vm.ID = customer.ID;
                    vm.FirstName = customer.Contact.FirstName;
                    vm.LastName = customer.Contact.LastName;
                    vm.Email = customer.Contact.Email;

                    vm.Phone1 = customer.Contact.Phone;
                    vm.Phone1Type = customer.Contact.PhoneNumberType1;
                    vm.Extension1 = customer.Contact.Extension1;

                    vm.Phone2 = customer.Contact.Phone2;
                    vm.Phone2Type = customer.Contact.PhoneNumberType2;
                    vm.Extension2 = customer.Contact.Extension2;
                    
                    vm.Phone3 = customer.Contact.Phone3;
                    vm.Phone3Type = customer.Contact.PhoneNumberType3;
                    vm.Extension3 = customer.Contact.Extension3;

                    vm.BusinessName = customer.Contact.BusinessName;
                    vm.Address1 = customer.Address.Line1;
                    vm.Address2 = customer.Address.Line2;
                    vm.City = customer.Address.City;
                    vm.State = customer.Address.State;
                    vm.Zip = customer.Address.Zip;
                    vm.TimeZone = customer.Address.TimeZone;
                }
            }

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteCustomer(int userID, int customerID)
        {
            if (IsUserAuthorized(userID))
            {
                CacheActiveLoginID(userID);

                Customer customer = Customer.Get(customerID);

                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                if (customer != null && customer.LoginID == activeLogin.LoginID)
                {
                    customer.IsDeleted = true;
                    SaveResult saveResult = customer.Save(activeLogin.ID);
                    if (saveResult.Success)
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(saveResult.ErrorMessage, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(@Proestimator.Resources.ProStrings.UnauthorizedDeleteCustomerRequest, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RestoreCustomer(int userID, int customerID)
        {
            if (IsUserAuthorized(userID))
            {
                CacheActiveLoginID(userID);

                Customer customer = Customer.Get(customerID);

                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                if (customer != null && customer.LoginID == activeLogin.LoginID)
                {
                    customer.IsDeleted = false;
                    SaveResult saveResult = customer.Save(activeLogin.ID);
                    if (saveResult.Success)
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(saveResult.ErrorMessage, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(@Proestimator.Resources.ProStrings.UnauthorizedRestoreCustomerRequest, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GridDeleteCustomers(int userID, string ids)
        {
            if (!IsUserAuthorized(userID))
            {
                return Json(@Proestimator.Resources.ProStrings.UnauthorizedLoginID, JsonRequestBehavior.AllowGet);
            }
            else
            {
                CacheActiveLoginID(userID);

                if (!string.IsNullOrEmpty(ids))
                {
                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                    List<string> idPieces = ids.Split(',').ToList();

                    foreach (string id in idPieces)
                    {
                        
                        int customerID = InputHelper.GetInteger(id);
                        Customer customer = Customer.Get(customerID);
                        if (customer != null && customer.LoginID == activeLogin.LoginID)
                        {
                            customer.IsDeleted = true;
                            customer.Save(activeLogin.ID);
                        }
                    }

                    return Json("success", JsonRequestBehavior.AllowGet);
                }

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GridRestoreCustomers(int userID, string ids)
        {
            if (!IsUserAuthorized(userID))
            {
                return Json(@Proestimator.Resources.ProStrings.UnauthorizedLoginID, JsonRequestBehavior.AllowGet);
            }
            else
            {
                CacheActiveLoginID(userID);

                if (!string.IsNullOrEmpty(ids))
                {
                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                    List<string> idPieces = ids.Split(',').ToList();

                    foreach (string id in idPieces)
                    {
                        int customerID = InputHelper.GetInteger(id);
                        Customer customer = Customer.Get(customerID);
                        if (customer != null && customer.LoginID == activeLogin.LoginID)
                        {
                            customer.IsDeleted = false;
                            customer.Save(activeLogin.ID);
                        }
                    }

                    return Json("success", JsonRequestBehavior.AllowGet);
                }

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public class SaveCustomerResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public int CustomerID { get; set; }
        }

        public JsonResult SaveCustomer(
            int userID
            , int customerID
            , string firstName
            , string lastName
            , string email
            , string email2

            , string businessName

            , string phone1
            , string ext1
            , string phoneType1

            , string phone2
            , string ext2
            , string phoneType2

            , string phone3
            , string ext3
            , string phoneType3

            , string address1
            , string address2
            , string city
            , string state
            , string zip
            , string timeZone)
        {
            CacheActiveLoginID(userID);

            SaveCustomerResult result = new SaveCustomerResult();
            result.Success = false;
            result.ErrorMessage = @Proestimator.Resources.ProStrings.LoginCustomerIdNotSaved;
            result.CustomerID = 0;

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                Customer customer = Customer.Get(customerID);

                if (customer == null)
                {
                    customer = new Customer();
                    customer.LoginID = activeLogin.LoginID;
                }

                if (customer != null && customer.LoginID == activeLogin.LoginID)
                {
                    customer.Contact.FirstName = firstName;
                    customer.Contact.LastName = lastName;
                    customer.Contact.Email = email;
                    customer.Contact.SecondaryEmail = email2;

                    customer.Contact.Phone = phone1;
                    customer.Contact.Extension1 = ext1;
                    customer.Contact.PhoneNumberType1 = phoneType1;

                    customer.Contact.Phone2 = phone2;
                    customer.Contact.Extension2 = ext2;
                    customer.Contact.PhoneNumberType2 = phoneType2;

                    customer.Contact.Phone3 = phone3;
                    customer.Contact.Extension3 = ext3;
                    customer.Contact.PhoneNumberType3 = phoneType3;

                    customer.Address.Line1 = address1;
                    customer.Address.Line2 = address2;
                    customer.Address.City = city;
                    customer.Address.State = state;
                    customer.Address.Zip = zip;
                    customer.Address.TimeZone = timeZone;

                    customer.Contact.BusinessName = businessName;

                    SaveResult saveResult = customer.Save(activeLogin.ID);

                    if (saveResult.Success)
                    {
                        result.Success = true;
                        result.ErrorMessage = "";
                        result.CustomerID = customer.ID;
                    }
                    else
                    {
                        result.ErrorMessage = saveResult.ErrorMessage;
                    }
                }
            }
                        
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<SelectListItem> GetStates()
        {
            var GetStatesData = State.StatesList.Select(s => (s.Code != "") ? new SelectListItem()
            {
                Text = s.Description,
                Value = s.Code
            } : new SelectListItem()
            {
                Selected = true,
                Text = "-----Select State-----",
                Value = s.Code
            });

            return GetStatesData;
        }

        #endregion

    }
}