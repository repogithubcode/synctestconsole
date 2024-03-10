using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class InsuranceVM
    {
        public string InsuranceCompanyName { get; set; }
        public int InsuranceCompanyID { get; set; }
        public bool UseLinkedInsuranceCompany { get; set; }
        public bool SaveInsuranceInfo { get; set; }

        public int UserID { get; set; }
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public bool HasEMSContract { get; set; }

        public ContactVM AgentVM { get; set; }
        public ContactVM ClaimantVM { get; set; }
        public AddressVM ClaimantAddressVM { get; set; }
        public ContactVM InsuredVM { get; set; }
        public AddressVM InsuredAddressVM { get; set; }

        public string PolicyNumber { get; set; }
        public string ClaimNumber { get; set; }
        public string Deduction { get; set; }
        public int CoverageType { get; set; }
        public string DateOfLoss { get; set; }

        public bool ClaimantSameAsOwner { get; set; }
        public string ClaimantPreference { get; set; }
        public int ExistingClaimantID { get; set; }
        public string ClaimantNotes { get; set; }

        public bool InsuredSameAsOwner { get; set; }
        public string InsuredPreference { get; set; }
        public int ExistingInsuredID { get; set; }
        public string InsuredNotes { get; set; }

        public SelectList InsuranceCompanies { get; set; }

        public List<SelectListItem> AdjustersList { get; set; }
        public ContactVM AdjusterVM { get; set; }
        public bool UseSavedAdjuster { get; set; }
        public bool SaveAdjusterInfo { get; set; }
        public int SavedAdjusterID { get; set; }

        public List<SelectListItem> ClaimRepList { get; set; }
        public ContactVM ClaimRepVM { get; set; }
        public bool UseSavedClaimRep { get; set; }
        public bool SaveClaimRepInfo { get; set; }
        public int SavedClaimRepID { get; set; }

        public bool IsDeleted { get; set; }
        public string DeleteRestoreImgName { get; set; }

        public bool PrintInsured { get; set; }

        public bool EstimateIsLocked { get; set; }

        public InsuranceVM()
        {

        }

        public InsuranceVM(InsuranceCompany insuranceCompany)
        {
            this.InsuranceCompanyName = insuranceCompany.Name;
            this.InsuranceCompanyID = insuranceCompany.ID;
            this.IsDeleted = insuranceCompany.IsDeleted;

            if (IsDeleted)
            {
                DeleteRestoreImgName = "restore.gif";
            }
            else
            {
                DeleteRestoreImgName = "delete.gif";
            }
        }

        public void LoadFromModel(Estimate estimate, InsuranceInfo insuranceInfo)
        {
            CoverageType = insuranceInfo.CoverageType;
            DateOfLoss = insuranceInfo.DateOfLoss.HasValue ? insuranceInfo.DateOfLoss.Value.ToShortDateString() : "";
            Deduction = Math.Round(insuranceInfo.Deduction, 2).ToString();
            PolicyNumber = insuranceInfo.PolicyNumber;
            ClaimNumber = insuranceInfo.ClaimNumber;
            ClaimantSameAsOwner = insuranceInfo.ClaimantSameAsOwner;
            InsuredSameAsOwner = insuranceInfo.InsuredSameAsOwner;
            InsuranceCompanyName = insuranceInfo.InsuranceCompanyName;

            AgentVM = new ContactVM(insuranceInfo.Agent);
            AdjusterVM = new ContactVM(insuranceInfo.Adjuster);
            ClaimRepVM = new ContactVM(insuranceInfo.ClaimRep);
            ClaimantVM = new ContactVM(insuranceInfo.Claimant);
            InsuredVM = new ContactVM(insuranceInfo.Insured);
            ClaimantAddressVM = new AddressVM(insuranceInfo.ClaimantAddress);
            InsuredAddressVM = new AddressVM(insuranceInfo.InsuredAddress);

            LoginID = estimate.CreatedByLoginID;
            EstimateID = estimate.EstimateID;
            PrintInsured = estimate.PrintInsured;

            // Setup saved insurance companies
            List<InsuranceCompany> insuranceCompanies = InsuranceCompany.GetForLogin(estimate.CreatedByLoginID);

            InsuranceCompany emptyCompany = new InsuranceCompany();
            emptyCompany.Name =  String.Format(@Proestimator.Resources.ProStrings.SelectSavedCategoryItem, "Insurance Company");
            insuranceCompanies.Insert(0, emptyCompany);
            
            InsuranceCompanies = new SelectList(insuranceCompanies, "ID", "Name");    

            if (estimate.InsuranceCompanyID > 0 && insuranceCompanies.FirstOrDefault(o => o.ID == estimate.InsuranceCompanyID) != null)
            {
                InsuranceCompanyID = estimate.InsuranceCompanyID;
                UseLinkedInsuranceCompany = true;
                InsuranceCompanyName = "";
            }

            // Set up the saved Adjusters
            AdjustersList = new List<SelectListItem>();
            string adjusterText = String.Format(@Proestimator.Resources.ProStrings.SelectSavedCategoryItem, "Adjuster");
            AdjustersList.Add(new SelectListItem() { Value = "0", Text = adjusterText });

            if (InsuranceCompanyID > 0)
            {
                List<InsuranceCompanyEmployee> savedAdjusters = InsuranceCompanyEmployee.GetForCompany(InsuranceCompanyID, InsuranceCompanyEmployeeType.Adjuster);
                if (savedAdjusters.Count > 0)
                {
                    foreach (InsuranceCompanyEmployee adjuster in savedAdjusters)
                    {
                        AdjustersList.Add(new SelectListItem() { Value = adjuster.ID.ToString(), Text = adjuster.FirstName + " " + adjuster.LastName });
                    }

                    if (estimate.AdjusterID > 0 && savedAdjusters.FirstOrDefault(o => o.ID == estimate.AdjusterID) != null)
                    {
                        SavedAdjusterID = estimate.AdjusterID;
                        UseSavedAdjuster = true;
                    }
                }
            }

            // Set up the saved ClaimReps
            ClaimRepList = new List<SelectListItem>();
            string claimRepText = String.Format(@Proestimator.Resources.ProStrings.SelectSavedCategoryItem, "Claim Rep");
            ClaimRepList.Add(new SelectListItem() { Value = "0", Text = claimRepText });

            if (InsuranceCompanyID > 0)
            {
                List<InsuranceCompanyEmployee> savedClaimReps = InsuranceCompanyEmployee.GetForCompany(InsuranceCompanyID, InsuranceCompanyEmployeeType.ClaimRep);
                if (savedClaimReps.Count > 0)
                {
                    foreach (InsuranceCompanyEmployee claimRep in savedClaimReps)
                    {
                        ClaimRepList.Add(new SelectListItem() { Value = claimRep.ID.ToString(), Text = claimRep.FirstName + " " + claimRep.LastName });
                    }

                    if (estimate.ClaimRepID > 0 && savedClaimReps.FirstOrDefault(o => o.ID == estimate.ClaimRepID) != null)
                    {
                        SavedClaimRepID = estimate.ClaimRepID;
                        UseSavedClaimRep = true;
                    }
                }
            }
        }
    }
}