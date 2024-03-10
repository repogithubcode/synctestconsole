using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData;
using ProEstimatorData.DataModel;

using Proestimator.ViewModel;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace Proestimator.Controllers
{
    public class InsuranceController : SiteController
    {
        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/insurance")]
        public ActionResult Insurance(int userID, int estimateID)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            // Make sure the estimate is attached to a customer
            if (estimate.CustomerID == 0)
            {
                return Redirect("/" + userID + "/estimate/" + estimate.EstimateID + "/customer-selection");
            }

            InsuranceInfo insuranceInfo = InsuranceInfo.Get(estimateID);

            InsuranceVM model = new InsuranceVM();
            model.LoadFromModel(estimate, insuranceInfo);
            model.HasEMSContract = ActiveLogin.HasEMSContract;
            model.UserID = userID;
            model.EstimateIsLocked = estimate.IsLocked();

            ViewBag.States = new SelectList(GetStates(), "Value", "Text");

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "insurance";

            ViewBag.EstimateID = estimateID;

            return View(model);
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

        /// <summary>
        /// Returns the list of insurance companies, used to bind to the grid
        /// </summary>
        public ActionResult GetInsuranceCompanies([DataSourceRequest] DataSourceRequest request, int userID, bool showDeletedCompanies)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            List<InsuranceCompany> insuranceCompanies = InsuranceCompany.GetForLogin(activeLogin.LoginID, showDeletedCompanies);

            List<InsuranceVM> listInsuranceVMs = new List<InsuranceVM>();

            foreach (InsuranceCompany insuranceCompany in insuranceCompanies)
            {
                InsuranceVM insuranceVM = new InsuranceVM(insuranceCompany);
                listInsuranceVMs.Add(insuranceVM);
            }

            return Json(listInsuranceVMs.OrderByDescending(o => o.InsuranceCompanyName).ToDataSourceResult(request));
        }

        /// <summary>
        /// Returns the list of saved insurance adjusters, used to bind to the grid
        /// </summary>
        public ActionResult GetAdjusters([DataSourceRequest] DataSourceRequest request, int insuranceCompanyID, bool showDeletedAdjusters)
        {
            List<InsuranceCompanyEmployeeVM> Adjusters = null;
            Adjusters = new List<InsuranceCompanyEmployeeVM>();
            List<InsuranceCompanyEmployee> adjusters = InsuranceCompanyEmployee.GetForCompany(insuranceCompanyID, InsuranceCompanyEmployeeType.Adjuster, showDeletedAdjusters);
            adjusters.ForEach(o => Adjusters.Add(new InsuranceCompanyEmployeeVM(o)));

            return Json(Adjusters.OrderByDescending(o => o.FullName).ToDataSourceResult(request));
        }

        /// <summary>
        /// Returns the list of saved insurance claim reps, used to bind to the grid
        /// </summary>
        public ActionResult GetClaimReps([DataSourceRequest] DataSourceRequest request, int insuranceCompanyID, bool showDeletedClaimReps)
        {
            List<InsuranceCompanyEmployeeVM> ClaimReps = null;
            ClaimReps = new List<InsuranceCompanyEmployeeVM>();
            List<InsuranceCompanyEmployee> claimReps = InsuranceCompanyEmployee.GetForCompany(insuranceCompanyID, InsuranceCompanyEmployeeType.ClaimRep, showDeletedClaimReps);
            claimReps.ForEach(o => ClaimReps.Add(new InsuranceCompanyEmployeeVM(o)));

            return Json(ClaimReps.OrderByDescending(o => o.FullName).ToDataSourceResult(request));
        }

        public JsonResult GetInsuranceCompanyData(int userID, int insuranceCompanyID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                InsuranceCompany insuranceCompany = InsuranceCompany.Get(insuranceCompanyID);
                if (insuranceCompany != null)
                {
                    InsuranceCompanyVM vm = new InsuranceCompanyVM(insuranceCompany);
                    return Json(vm, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetInsuranceEmployee(int userID, int id)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                InsuranceCompanyEmployee employee = InsuranceCompanyEmployee.Get(id);
                if (employee != null)
                {
                    employee.ClearRowAsLoaded();
                    return Json(employee, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("{userID}/estimate/{estimateID}/insurance")]
        public ActionResult Insurance(InsuranceVM model)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(model.EstimateID);
            if (estimate.CreatedByLoginID != model.LoginID)
            {
                return Redirect("/" + model.UserID);
            }

            SaveInsuranceInfo(model);
            return DoRedirect("insurance");
        }

        private void SaveInsuranceInfo(InsuranceVM model)
        {
            InsuranceInfo insuranceInfo = InsuranceInfo.Get(model.EstimateID);

            insuranceInfo.CoverageType = model.CoverageType;
            insuranceInfo.DateOfLoss = InputHelper.GetNullableDateTime(model.DateOfLoss);
            insuranceInfo.Deduction = InputHelper.GetDecimal(model.Deduction);
            insuranceInfo.PolicyNumber = model.PolicyNumber;
            insuranceInfo.ClaimNumber = model.ClaimNumber;
            insuranceInfo.ClaimantSameAsOwner = model.ClaimantSameAsOwner;
            insuranceInfo.InsuredSameAsOwner = model.InsuredSameAsOwner;

            if (model.AgentVM != null) { model.AgentVM.CopyToContact(insuranceInfo.Agent); }
            if (model.ClaimantVM != null) { model.ClaimantVM.CopyToContact(insuranceInfo.Claimant); }
            if (model.InsuredVM != null) { model.InsuredVM.CopyToContact(insuranceInfo.Insured); }
            if (model.ClaimantAddressVM != null) { model.ClaimantAddressVM.CopyToAddress(insuranceInfo.ClaimantAddress); }
            if (model.InsuredAddressVM != null) { model.InsuredAddressVM.CopyToAddress(insuranceInfo.InsuredAddress); }

            Estimate estimate = new Estimate(model.EstimateID);
            estimate.PrintInsured = model.PrintInsured;

            if (model.UseLinkedInsuranceCompany)
            {
                InsuranceCompany insuranceCompany = InsuranceCompany.Get(model.InsuranceCompanyID);
                if (insuranceCompany != null)
                {
                    insuranceInfo.InsuranceCompanyName = insuranceCompany.Name;
                    estimate.InsuranceCompanyID = model.InsuranceCompanyID;
                }
            }
            else
            {
                insuranceInfo.InsuranceCompanyName = model.InsuranceCompanyName;
                estimate.InsuranceCompanyID = 0;

                if (model.SaveInsuranceInfo && !string.IsNullOrEmpty(model.InsuranceCompanyName))
                {
                    List<InsuranceCompany> insuranceCompanies = InsuranceCompany.GetForLogin(model.LoginID);
                    if (insuranceCompanies.FirstOrDefault(o => o.Name == model.InsuranceCompanyName) == null)
                    {
                        InsuranceCompany newInsuranceCompany = new InsuranceCompany();
                        newInsuranceCompany.Name = model.InsuranceCompanyName;
                        newInsuranceCompany.LoginID = model.LoginID;
                        newInsuranceCompany.Save(GetActiveLoginID(model.UserID));

                        estimate.InsuranceCompanyID = newInsuranceCompany.ID;
                        insuranceInfo.InsuranceCompanyName = "";
                    }
                }
            }

            // Update the Adjuster
            InsuranceCompanyEmployee savedAdjuster = null;

            if (model.SavedAdjusterID > 0)
            {
                // If a saved adjuster was picked, link to it and copy its data.  
                estimate.AdjusterID = model.SavedAdjusterID;
                savedAdjuster = InsuranceCompanyEmployee.Get(model.SavedAdjusterID);
            }
            else
            {
                // No saved adjuster picked, copy fields from form
                estimate.AdjusterID = 0;
                if (model.AdjusterVM != null) { model.AdjusterVM.CopyToContact(insuranceInfo.Adjuster); }
            }

            // Save a new adjuster
            if (model.SaveAdjusterInfo && model.SavedAdjusterID == 0 && model.InsuranceCompanyID > 0)
            {
                List<InsuranceCompanyEmployee> allAdjusters = InsuranceCompanyEmployee.GetForCompany(model.InsuranceCompanyID, InsuranceCompanyEmployeeType.Adjuster);
                savedAdjuster = allAdjusters.FirstOrDefault(o => o.FirstName == insuranceInfo.Adjuster.FirstName && o.LastName == insuranceInfo.Adjuster.LastName);

                if (savedAdjuster == null)
                {
                    savedAdjuster = new InsuranceCompanyEmployee();
                }

                savedAdjuster.FirstName = insuranceInfo.Adjuster.FirstName;
                savedAdjuster.LastName = insuranceInfo.Adjuster.LastName;
                savedAdjuster.Phone = insuranceInfo.Adjuster.Phone;
                savedAdjuster.Extension = insuranceInfo.Adjuster.Extension1;
                savedAdjuster.Fax = insuranceInfo.Adjuster.Fax;
                savedAdjuster.Email = insuranceInfo.Adjuster.Email;
                savedAdjuster.LoginID = model.LoginID;
                savedAdjuster.InsuranceCompanyID = model.InsuranceCompanyID;
                savedAdjuster.EmployeeType = InsuranceCompanyEmployeeType.Adjuster;
                savedAdjuster.Save(GetActiveLoginID(model.UserID));

                estimate.AdjusterID = savedAdjuster.ID;
            }

            if (savedAdjuster != null)
            {
                insuranceInfo.Adjuster.FirstName = savedAdjuster.FirstName;
                insuranceInfo.Adjuster.LastName = savedAdjuster.LastName;
                insuranceInfo.Adjuster.Phone = savedAdjuster.Phone;
                insuranceInfo.Adjuster.Extension1 = savedAdjuster.Extension;
                insuranceInfo.Adjuster.Fax = savedAdjuster.Fax;
                insuranceInfo.Adjuster.Email = savedAdjuster.Email;
            }

            // Update the ClaimRep
            InsuranceCompanyEmployee savedClaimRep = null;

            if (model.SavedClaimRepID > 0)
            {
                // If a saved claim rep was picked, link to it and copy its data.  
                estimate.ClaimRepID = model.SavedClaimRepID;
                savedClaimRep = InsuranceCompanyEmployee.Get(model.SavedClaimRepID);
            }
            else
            {
                // No saved claim rep picked, copy fields from form
                estimate.ClaimRepID = 0;
                if (model.ClaimRepVM != null) { model.ClaimRepVM.CopyToContact(insuranceInfo.ClaimRep); }
            }

            // Save a new claim rep
            if (model.SaveClaimRepInfo && model.SavedClaimRepID == 0 && model.InsuranceCompanyID > 0)
            {
                List<InsuranceCompanyEmployee> allClaimReps = InsuranceCompanyEmployee.GetForCompany(model.InsuranceCompanyID, InsuranceCompanyEmployeeType.ClaimRep);
                savedClaimRep = allClaimReps.FirstOrDefault(o => o.FirstName == insuranceInfo.ClaimRep.FirstName && o.LastName == insuranceInfo.ClaimRep.LastName);

                if (savedClaimRep == null)
                {
                    savedClaimRep = new InsuranceCompanyEmployee();
                }

                savedClaimRep.FirstName = insuranceInfo.ClaimRep.FirstName;
                savedClaimRep.LastName = insuranceInfo.ClaimRep.LastName;
                savedClaimRep.Phone = insuranceInfo.ClaimRep.Phone;
                savedClaimRep.Extension = insuranceInfo.ClaimRep.Extension1;
                savedClaimRep.Fax = insuranceInfo.ClaimRep.Fax;
                savedClaimRep.Email = insuranceInfo.ClaimRep.Email;
                savedClaimRep.LoginID = model.LoginID;
                savedClaimRep.InsuranceCompanyID = model.InsuranceCompanyID;
                savedClaimRep.EmployeeType = InsuranceCompanyEmployeeType.ClaimRep;
                savedClaimRep.Save(GetActiveLoginID(model.UserID));

                estimate.ClaimRepID = savedClaimRep.ID;
            }

            if (savedClaimRep != null)
            {
                insuranceInfo.ClaimRep.FirstName = savedClaimRep.FirstName;
                insuranceInfo.ClaimRep.LastName = savedClaimRep.LastName;
                insuranceInfo.ClaimRep.Phone = savedClaimRep.Phone;
                insuranceInfo.ClaimRep.Extension1 = savedClaimRep.Extension;
                insuranceInfo.ClaimRep.Fax = savedClaimRep.Fax;
                insuranceInfo.ClaimRep.Email = savedClaimRep.Email;
            }

            int activeLoginID = GetActiveLoginID(model.UserID);

            estimate.Save(activeLoginID);
            insuranceInfo.Save(activeLoginID, model.EstimateID, model.LoginID);
        }

        public class SaveResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public int ID { get; set; }
        }

        public JsonResult SaveInsuranceCompany(int userID, int insuranceCompanyID, string insuranceCompanyName)
        {
            CacheActiveLoginID(userID);

            SaveResult result = new SaveResult();

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                if (!string.IsNullOrEmpty(insuranceCompanyName))
                {
                    List<InsuranceCompany> insuranceCompanies = InsuranceCompany.GetForLogin(activeLogin.LoginID);
                    if (insuranceCompanies.FirstOrDefault(o => o.Name == insuranceCompanyName) == null)
                    {
                        InsuranceCompany newInsuranceCompany = new InsuranceCompany(insuranceCompanyID);
                        newInsuranceCompany.Name = insuranceCompanyName;
                        newInsuranceCompany.LoginID = activeLogin.LoginID;
                        var functionResult = newInsuranceCompany.Save(activeLogin.ID);

                        if (functionResult.Success)
                        {
                            result.Success = true;
                            result.ErrorMessage = "";
                        }
                        else
                        {
                            result.ErrorMessage = functionResult.ErrorMessage;
                        }
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveInsuranceEmployee(int userID, int loginID, int insuranceCompanyID, string employeeType, int employeeID, string firstName,
                                                                string lastName, string phone, string extension, string fax, string email)
        {
            CacheActiveLoginID(userID);

            SaveResult result = new SaveResult();

            InsuranceCompanyEmployee savedEmployee = null;

            if (insuranceCompanyID > 0)
            {
                List<InsuranceCompanyEmployee> allEmployees = null;

                if (employeeType == "adjuster")
                {
                    allEmployees = InsuranceCompanyEmployee.GetForCompany(insuranceCompanyID, InsuranceCompanyEmployeeType.Adjuster);
                }
                else if (employeeType == "claimrep")
                {
                    allEmployees = InsuranceCompanyEmployee.GetForCompany(insuranceCompanyID, InsuranceCompanyEmployeeType.ClaimRep);
                }

                // savedEmployee = allEmployees.FirstOrDefault(o => o.FirstName == firstName && o.LastName == lastName);
                savedEmployee = allEmployees.FirstOrDefault(o => o.ID == employeeID);

                if (savedEmployee == null)
                {
                    savedEmployee = new InsuranceCompanyEmployee();
                }

                savedEmployee.FirstName = firstName;
                savedEmployee.LastName = lastName;
                savedEmployee.Phone = phone;
                savedEmployee.Extension = extension;
                savedEmployee.Fax = fax;
                savedEmployee.Email = email;
                savedEmployee.LoginID = loginID;
                savedEmployee.InsuranceCompanyID = insuranceCompanyID;

                if (employeeType == "adjuster")
                {
                    savedEmployee.EmployeeType = InsuranceCompanyEmployeeType.Adjuster;
                }
                else if (employeeType == "claimrep")
                {
                    savedEmployee.EmployeeType = InsuranceCompanyEmployeeType.ClaimRep;
                }

                var functionResult = savedEmployee.Save(GetActiveLoginID(userID));

                if (functionResult.Success)
                {
                    result.Success = true;
                    result.ErrorMessage = "";
                }
                else
                {
                    result.ErrorMessage = functionResult.ErrorMessage;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteInsuranceCompany(int userID, int insuranceCompanyID, Boolean isDeleted)
        {
            CacheActiveLoginID(userID);

            string errorMessage = "";

            if (IsUserAuthorized(userID))
            {
                InsuranceCompany insuranceCompany = InsuranceCompany.Get(insuranceCompanyID);
                if (insuranceCompany != null)
                {
                    insuranceCompany.IsDeleted = !isDeleted;
                    insuranceCompany.Save(GetActiveLoginID(userID));
                }
                else
                {
                    errorMessage = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingReportInvalidReportID, insuranceCompanyID);
                }
            }

            return Json(errorMessage, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteInsuranceEmployee(int userID, int employeeID, Boolean isDeleted)
        {
            CacheActiveLoginID(userID);

            string errorMessage = "";

            if (IsUserAuthorized(userID))
            {
                InsuranceCompanyEmployee employee = InsuranceCompanyEmployee.Get(employeeID);
                if (employee != null)
                {
                    employee.IsDeleted = !isDeleted;
                    employee.Save(GetActiveLoginID(userID));
                }
                else
                {
                    errorMessage = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingReportInvalidReportID, employeeID);
                }
            }

            return Json(errorMessage, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns the list of insurance companies when model dialog is closed
        /// </summary>
        public ActionResult GetInsuranceCompaniesWithEmptyCompany(int loginID)
        {
            // Setup saved insurance companies
            List<InsuranceCompany> insuranceCompanies = InsuranceCompany.GetForLogin(loginID);

            InsuranceCompany emptyCompany = new InsuranceCompany();
            emptyCompany.Name = String.Format(@Proestimator.Resources.ProStrings.SelectSavedCategoryItem, "Insurance Company");
            insuranceCompanies.Insert(0, emptyCompany);

            List<InsuranceVM> listInsuranceVMs = new List<InsuranceVM>();

            foreach (InsuranceCompany insuranceCompany in insuranceCompanies)
            {
                InsuranceVM insuranceVM = new InsuranceVM(insuranceCompany);
                listInsuranceVMs.Add(insuranceVM);
            }

            return Json(listInsuranceVMs, JsonRequestBehavior.AllowGet);
        }
    }
}