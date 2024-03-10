using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.SqlClient;

using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.ProAdvisor;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.DataModel.Profiles;

using ProEstimator.DataRepositories.Vendors;
using ProEstimator.Business.ProAdvisor;

namespace Proestimator.ViewModel
{
    public class DetailsVM
    {
        public int UserID { get; set; }
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public bool CanChangeDate { get; set; }

        public int OriginalRateProfileID { get; set; }
        public int RateProfileID { get; set; }
        public SelectList RateProfileSelections { get; set; }

        public bool UseAddOns { get; set; }
        public int AddOnProfileID { get; set; }
        public SelectList AddOnProfileSelections { get; set; }

        public bool HasPDRRateProfile { get; set; }
        public int PDRRateProfileID { get; set; }
        public SelectList PDRRateProfileSelections { get; set; }

        public int EstimatorID { get; set; }
        public SelectList EstimatorSelections { get; set; }
        public bool HasEstimators { get; set; }

        public string SelectedReportHeader { get; set; }
        public SelectList ReportHeaderSelections { get; set; }

        public string EstimateDescription { get; set; }
        public string EstimateLocation { get; set; }
        [AllowHtml]
        public string EstimateNotes { get; set; }
        public bool IncludeNotesInReport { get; set; }
        public string EstimatorOrAppraiser { get; set; }

        public string InspectionDate { get; set; }
        public string AssignmentDate { get; set; }

        public SelectList RepairFacilities { get; set; }
        public int SelectedRepairFacility { get; set; }
        public bool ShowRepairFacilities { get; set; }

        public bool UseAlternateIdentities { get; set; }
        public SelectList AlternateIdentities { get; set; }
        public int SelectedAlternateIdentity { get; set; }

        public SelectList Statuses { get; set; }
        public int SelectedStatus { get; set; }

        public string StatusDate { get; set; }

        public string CurrentSupplementLevel { get; set; }

        public List<JobStatusHistoryVM> StatusHistory { get; set; }

        public string RepairOrderNumber { get; set; }
        public string PurchaseOrderNumber { get; set; }

        public bool Print_Owner { get; set; }
        public bool Print_Claimepresentative  { get; set; }
        public bool Print_Insured { get; set; }
        public bool Print_Estimator { get; set; }
        public bool Print_InsuranceAgent { get; set; }
        public bool Print_Adjuster { get; set; }

        public string WhichButtonVisible { get; set; }
        public string DaysToRepair { get; set; }
        public bool ManualRepairDays { get; set; }
        public string HoursInDay { get; set; }
        public SelectList HoursInDaySelections { get; set; }
        public string NonUserDaysToRepair { get; set; }
        public string FacilityRepairOrder { get; set; }
        public string FacilityRepairInvoice { get; set; }
        public decimal CreditCardFeePercentage { get; set; }
        public bool ApplyCreditCardFee { get; set; }
        public Boolean TaxedCreditCardFee { get; set; }

        public bool EstimateIsLocked { get; set; }
        public DetailsVM() { }

        public void FillLists(IVendorRepository vendorService, IProAdvisorProfileService proAdvisorProfileService, Estimate estimate, bool hasAddOnContract, int activeLoginID)
        {
            // Fill the list of available statuses.
            List<JobStatusHistory> history = JobStatusHistory.GetForEstimate(estimate.EstimateID);
            StatusHistory = new List<JobStatusHistoryVM>();

            foreach(JobStatusHistory jobStatusHistory in history)
            {
                StatusHistory.Add(new JobStatusHistoryVM(jobStatusHistory));
            }

            // Fill the Rate Profiles selection list
            List<SelectListItem> selections = new List<SelectListItem>();
            List<RateProfile> profiles = RateProfile.GetAllForLogin(LoginID).OrderBy(o => o.Name).ToList();
            foreach (RateProfile rateProfile in profiles)
            {
                selections.Add(new SelectListItem() { Text = rateProfile.Name, Value = rateProfile.ID.ToString() });
            }

            // If the current profile doesn't have a parent or the parent isn't in the list, add the current profile to the list
            RateProfile currentProfile = RateProfile.Get(estimate.CustomerProfileID);
            RateProfile parentProfile = profiles.FirstOrDefault(o => o.ID == currentProfile.OriginalID);
            if (parentProfile == null)
            {
                selections.Add(new SelectListItem() { Text = currentProfile.Name, Value = currentProfile.ID.ToString() });
            }

            RateProfileSelections = new SelectList(selections, "Value", "Text");

            // Setup add ons 
            if (hasAddOnContract)
            {
                List<ProAdvisorPresetProfile> addOnProfiles = proAdvisorProfileService.GetAllProfilesForLogin(LoginID, false);
                List<SelectListItem> addOnProfileItems = new List<SelectListItem>();

                addOnProfileItems.Add(new SelectListItem() { Text = "System Default", Value = "1" });

                foreach(ProAdvisorPresetProfile profile in addOnProfiles)
                {
                    addOnProfileItems.Add(new SelectListItem() { Text = profile.Name, Value = profile.ID.ToString() });
                }

                AddOnProfileSelections = new SelectList(addOnProfileItems, "Value", "Text");

                AddOnProfileID = estimate.AddOnProfileID;
                UseAddOns = true;
            }

            // Get the original rate profile ID so it can be selected from the list of all rate profiles
            if ((currentProfile.OriginalID == currentProfile.ID) || (parentProfile == null))
            {
                RateProfileID = currentProfile.ID;
            }
            else
            {
                RateProfileID = RateProfile.GetOriginalProfileID(estimate.EstimateID);
            }
            OriginalRateProfileID = RateProfileID;

            // Fill the PDR Rate Profiles list if applicable
            PDR_EstimateData pdrEstimateData = PDR_EstimateData.GetForEstimate(EstimateID);
            if (pdrEstimateData != null)
            {
                if (pdrEstimateData.RateProfileID > 0)
                {
                    PDR_RateProfile pdrRateProfile = PDR_RateProfile.GetByID(pdrEstimateData.RateProfileID);
                    PDRRateProfileID = pdrRateProfile.OriginalID;
                    HasPDRRateProfile = true;
                }                

                List<PDR_RateProfile> pdrRateProfiles = PDR_RateProfile.GetByLogin(LoginID);

                List<SelectListItem> pdrSelections = new List<SelectListItem>();
                foreach (PDR_RateProfile rateProfile in pdrRateProfiles)
                {
                    pdrSelections.Add(new SelectListItem() { Text = rateProfile.ProfileName, Value = rateProfile.ID.ToString() });
                }
                PDRRateProfileSelections = new SelectList(pdrSelections, "Value", "Text");
            }

            // Fill the Estimators list
            List<SelectListItem> estimatorSelections = new List<SelectListItem>();

            List<Estimator> estimators = Estimator.GetByLogin(LoginID, activeLoginID).OrderBy(o => o.FirstName + " " + o.LastName).ToList();
            if (estimators.Count > 0)
            {
                foreach (Estimator estimator in estimators)
                {
                    estimatorSelections.Add(new SelectListItem() { Text = estimator.FirstName + " " + estimator.LastName, Value = estimator.ID.ToString() });
                }

                HasEstimators = true;
            }
            EstimatorSelections = new SelectList(estimatorSelections, "Value", "Text");

            // Fill the report headers list
            List<SelectListItem> reportHeaders = new List<SelectListItem>();
            //reportHeaders.Add(new SelectListItem() { Text = Proestimator.Resources.ProStrings.SelectReportHeader, Value = "" });
            foreach (ReportHeader header in ReportHeader.ReportHeadersList)
            {
                reportHeaders.Add(new SelectListItem() { Text = header.Header, Value = header.Header });
            }
            ReportHeaderSelections = new SelectList(reportHeaders, "Value", "Text");

            // Get the repair facilities list from the Vendors table
            List<SelectListItem> repairFacilities = new List<SelectListItem>();
            repairFacilities.Add(new SelectListItem() { Text = "", Value = "0" });

            List<ProEstimatorData.DataModel.Vendor> repairFacilityVendors = vendorService.GetAllForType(LoginID, VendorType.RepairFacility);
            foreach (ProEstimatorData.DataModel.Vendor vendor in repairFacilityVendors)
            {
                repairFacilities.Add(new SelectListItem() { Text = vendor.CompanyName, Value = vendor.ID.ToString() });
            }

            RepairFacilities = new SelectList(repairFacilities, "Value", "Text");

            // Set up alternate identities if they are activeated for the user
            UseAlternateIdentities = false;

            LoginInfo loginInfo = LoginInfo.GetByID(LoginID);
            if (loginInfo != null && loginInfo.AllowAlternateIdentities)
            {
                UseAlternateIdentities = true;

                // Get the Alternate Identities list from the Vendors table            
                List<SelectListItem> alternateIdentities = new List<SelectListItem>();
                alternateIdentities.Add(new SelectListItem() { Text = "", Value = "0" });

                List<ProEstimatorData.DataModel.Vendor> repairFacilityAlternateIdentities = vendorService.GetAllForType(LoginID, VendorType.AlternateIdentity);
                foreach (ProEstimatorData.DataModel.Vendor vendor in repairFacilityAlternateIdentities)
                {
                    alternateIdentities.Add(new SelectListItem() { Text = vendor.CompanyName, Value = vendor.ID.ToString() });
                }
                AlternateIdentities = new SelectList(alternateIdentities, "Value", "Text");
            }
            
            EstimatorOrAppraiser = loginInfo.Appraiser ? @Proestimator.Resources.ProStrings.SelectAppraiser : @Proestimator.Resources.ProStrings.SelectEstimator;

            List<SelectListItem> statusItems = new List<SelectListItem>();
            statusItems.Add(new SelectListItem() { Text = "", Value = "0" });

            foreach (JobStatus jobStatus in JobStatus.GetForStatus(estimate.CurrentJobStatusID))
            {
                statusItems.Add(new SelectListItem() { Text = jobStatus.Description, Value = jobStatus.ID.ToString() });
            }

            int timezoneOffset = ProEstimatorData.InputHelper.GetInteger(ProEstimator.Business.Logic.SiteSettings.Get(LoginID, "TimeZone", "ReportOptions", "0").ValueString);
            StatusDate = DateTime.Now.AddHours(timezoneOffset).ToShortDateString();

            JobStatusHistory jobStatusHistoryObj = history.FirstOrDefault();
            if (jobStatusHistoryObj != null && jobStatusHistoryObj.JobStatus.ID == 3)
            {
                StatusDate = jobStatusHistoryObj.ActionDate.ToShortDateString();
            }

            Statuses = new SelectList(statusItems, "Value", "Text");
        }

        public void SetDaysToRepair(Estimate estimate)
        {
            ProEstimatorData.DBAccess db = new ProEstimatorData.DBAccess();
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AdminInfoID", EstimateID));
            parameters.Add(new SqlParameter("SupplementVersion", estimate.GetSupplementForReport()));

            ProEstimatorData.DBAccessStringResult result = db.ExecuteWithStringReturn("GetDaysToRepair", parameters);
            if (result.Success)
            {
                string temp = result.Value;
                int i = temp.IndexOf("(Supplement");
                if (i > -1)
                {
                    temp = temp.Substring(0, i - 1);
                }

                NonUserDaysToRepair = string.IsNullOrEmpty(temp) ? "0" : temp;   
            }
            else
            {
                NonUserDaysToRepair = "0";
            }

            if (estimate.RepairDays == -1)
            {
                DaysToRepair = Math.Ceiling(Convert.ToDecimal(NonUserDaysToRepair)).ToString();   
            }
            else
            {
                DaysToRepair = estimate.RepairDays.ToString();
                ManualRepairDays = true;
            }

            HoursInDay = estimate.HoursInDay.ToString();
            List<SelectListItem> hours = new List<SelectListItem>();
            hours.Add(new SelectListItem() { Text = "4", Value = "4" });
            hours.Add(new SelectListItem() { Text = "5", Value = "5" });
            hours.Add(new SelectListItem() { Text = "6", Value = "6" });
            HoursInDaySelections = new SelectList(hours, "Value", "Text");
        }

        public DetailsVM(IVendorRepository vendorService, IProAdvisorProfileService proAdvisorProfileService, Estimate estimate, bool hasAddOnContract, int activeLoginID)
        {
            EstimateID = estimate.EstimateID;
            LoginID = estimate.CreatedByLoginID;

            FillLists(vendorService, proAdvisorProfileService, estimate, hasAddOnContract, activeLoginID);

            // Fill the rest of the data fields
            EstimateDescription = estimate.Description;
            EstimateLocation = estimate.EstimateLocation;
            EstimateNotes = estimate.Note;
            IncludeNotesInReport = estimate.PrintNote;
            EstimatorID = estimate.EstimatorID;
            SelectedReportHeader = estimate.ReportTextHeader;
            SelectedRepairFacility = estimate.RepairFacilityVendorID;
            SelectedAlternateIdentity = estimate.AlternateIdentitiesID;
            CurrentSupplementLevel = estimate.LockLevel.ToString();
            RepairOrderNumber = estimate.WorkOrderNumber.ToString();
            PurchaseOrderNumber = estimate.PurchaseOrderNumber;
            FacilityRepairOrder = estimate.FacilityRepairOrder;
            FacilityRepairInvoice = estimate.FacilityRepairInvoice;

            InspectionDate = estimate.EstimationDate.HasValue ? estimate.EstimationDate.Value.ToShortDateString() : "";
            AssignmentDate = estimate.AssignmentDate.HasValue ? estimate.AssignmentDate.Value.ToShortDateString() : "";

            CreditCardFeePercentage = estimate.CreditCardFeePercentage;
            ApplyCreditCardFee = estimate.ApplyCreditCardFee;
            TaxedCreditCardFee = estimate.TaxedCreditCardFee;
            EstimateIsLocked = estimate.IsLocked();

            SetDaysToRepair(estimate);
        }
    }

    public class JobStatusHistoryVM
    {
        public string Description { get; set; }
        public string TimeStamp { get; set; }

        public JobStatusHistoryVM(JobStatusHistory jobStatusHistory)
        {
            Description = jobStatusHistory.JobStatus == null ? "" : jobStatusHistory.JobStatus.Description;
            TimeStamp = jobStatusHistory.ActionDate.ToShortDateString();
            
            if (jobStatusHistory.ActionDate.Hour != 0 || jobStatusHistory.ActionDate.Minute != 0 || jobStatusHistory.ActionDate.Second != 0)
            {
                TimeStamp += " " + jobStatusHistory.ActionDate.ToShortTimeString();
            }
            
        }
    }
}