using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.ViewModel.Logins
{
    public class LoginsPageVM
    {

        public int SessionSalesRepID { get; set; }

        public bool GoodData { get; set; }
        public int LoginID { get; set; }

        public bool CreateNewLogin { get; set; }

        public string ErrorMessage { get; set; }
        public string ExtraSaveMessage { get; set; }

        public DateTime CreationDate { get; set; }
        public string LoginName { get; set; }
        public string Organization { get; set; }
        public string Password { get; set; }
        public string TechSupportPassword { get; set; }

        public bool Disabled { get; set; }
        public bool DoubtfulAccount { get; set; }
        public bool StaffAccount { get; set; }
        public bool Appraiser { get; set; }

        public bool CarfaxExclude { get; set; }

        public string NoOfLogins { get; set; }

        public int CompanyOrigin { get; set; }
        public SelectList CompanyOrigins { get; set; }

        public string CompanyType { get; set; }
        public SelectList CompanyTypes { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string HeaderContact { get; set; }
        public string JobTitle { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneType1 { get; set; }
        public string PhoneNumber1 { get; set; }
        public string PhoneType2 { get; set; }
        public string PhoneNumber2 { get; set; }
        public string FaxNumber { get; set; }
        public SelectList PhoneTypes { get; set; }

        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public SelectList States { get; set; }
        public string Zip { get; set; }

        public int SalesRepID { get; set; }
        public int NewSalesRepID { get; set; }
        public SelectList SalesReps { get; set; }
        public SelectList NewSalesReps { get; set; }

        public string RegistrationNumber { get; set; }
        public bool ShowRepairShopProfiles { get; set; }
        public bool AllowAlternateIdentities { get; set; }        
        public bool ShowLaborTimeWO { get; set; }
        public string LicenseNumber { get; set; }
        public string BarNumber { get; set; }
        public string FederalTaxID { get; set; }
        public bool UseDefaultRateProfile { get; set; }
        public bool UseDefaultPDRRateProfile { get; set; }
        public string LogoImageType { get; set; }
        public bool ProfileLocked { get; set; }
        public bool PartsNow { get; set; }
        public bool OverlapAdmin { get; set; }
        public string LastEstimateNumber { get; set; }
        public string LastWorkOrderNumber { get; set; }

        public string IntelliPayMerchantKey { get; set; }
        public string IntelliPayAPIKey { get; set; }
        public bool IntelliPayUseCardReader { get; set; }

        // summary stuff
        public int NumberOfEstimates { get; set; }
        public bool HasActiveContract { get; set; }
        public string ContractDetails { get; set; }
        public string TotalDueDetails { get; set; }

        public string ImpersonateLink { get; set; }
        public SelectList ImpersonateLinks { get; set; }

        public int ScrollPosition { get; set; }
        public string SelectedTab { get; set; }

        public int SelectedStripeInfoID { get; set; }
        public bool CcInfoDeleteFlag { get; set; }

        public List<SiteUserPermissionVM> SitePermissions { get; set; }

        public bool GoodSave { get; set; }
        public bool IsAutoRenew { get; set; }

        public bool IsRemoteSupportOn { get; set; }

        public string CarfaxExcludeDateStr { get; set; }
        public Double ProAdvisorEstimateTotal { get; set; }
        public string ModifiedBySalesRepName { get; set; }
        public bool ContractActive { get; set; }
        public int LanguageID { get; set; }
        public SelectList LanguageList { get; set; }
        public String LastActivityTime { get; set; }

        public LoginsPageVM()
        {
            // Set defaults
            ProfileLocked = true;
            NoOfLogins = "1";

            // Company Types drop down
            List<SelectListItem> companyTypes = new List<SelectListItem>();
            companyTypes.Add(new SelectListItem() { Text = "Repair Shop", Value = "Repair" });
            companyTypes.Add(new SelectListItem() { Text = "Adjuster", Value = "Adjuster" });
            companyTypes.Add(new SelectListItem() { Text = "Insurer", Value = "Insurer" });

            CompanyTypes = new SelectList(companyTypes, "Value", "Text");

            // Company Origins drop down
            List<SelectListItem> companyOrigins = new List<SelectListItem>();
            companyOrigins.Add(new SelectListItem() { Value = "255", Text = "" });
            companyOrigins.Add(new SelectListItem() { Value = "10", Text = "ABF" });
            companyOrigins.Add(new SelectListItem() { Value = "5", Text = "Audatex" });
            companyOrigins.Add(new SelectListItem() { Value = "7", Text = "Books" });
            companyOrigins.Add(new SelectListItem() { Value = "4", Text = "CCC" });
            companyOrigins.Add(new SelectListItem() { Value = "3", Text = "Comp-Est" });
            companyOrigins.Add(new SelectListItem() { Value = "9", Text = "Comp-Est RG" });
            companyOrigins.Add(new SelectListItem() { Value = "1", Text = "Crash-Write" });
            companyOrigins.Add(new SelectListItem() { Value = "6", Text = "Hand Written" });
            companyOrigins.Add(new SelectListItem() { Value = "11", Text = "New Shop" });
            companyOrigins.Add(new SelectListItem() { Value = "8", Text = "Other" });
            companyOrigins.Add(new SelectListItem() { Value = "2", Text = "RepairMate" });
            companyOrigins.Add(new SelectListItem() { Value = "0", Text = "Web-Est" });
            CompanyOrigins = new SelectList(companyOrigins, "Value", "Text");

            // States drop down
            List<State> stateListColl = ProEstimatorData.DataModel.State.StatesList;
            States = new SelectList(stateListColl, "Code", "Description");
            //State stateOnEmptyCode = stateListColl.Where(eachState => (string.Compare(eachState.Code, "", true) == 0)).FirstOrDefault<State>();
            //stateListColl.Remove(stateOnEmptyCode);
            //stateListColl.Insert(0, new State("", "-- Select State --"));

            // Sales Rep drop down
            List<SelectListItem> salesRepSelectList = new List<SelectListItem>();
            List<ProEstimatorData.DataModel.SalesRep> salesReps = ProEstimatorData.DataModel.SalesRep.GetAll().Where(o => o.IsSalesRep && o.IsActive).ToList();
            foreach (ProEstimatorData.DataModel.SalesRep salesRep in salesReps)
            {
                salesRepSelectList.Add(new SelectListItem() { Value = salesRep.SalesRepID.ToString(), Text = salesRep.SalesNumber + " - " + salesRep.FirstName + " " + salesRep.LastName });
            }
            SalesReps = new SelectList(salesRepSelectList, "Value", "Text");

            // Add an empty for the New drop down
            List<SelectListItem> allSalesRepSelectList = salesRepSelectList.ToList();
            allSalesRepSelectList.Insert(0, new SelectListItem() { Value = "-1", Text = "--Select--" });
            NewSalesReps = new SelectList(allSalesRepSelectList, "Value", "Text");

            NewSalesRepID = -1;

            // Phone Types drop down
            List<PhoneType> phoneTypes = PhoneType.GetAll();
            phoneTypes.Insert(0, new PhoneType("", ""));
            PhoneTypes = new SelectList(phoneTypes, "Code", "ScreenDisplay");  

            SitePermissions = new List<SiteUserPermissionVM>();
            List<SitePermission> permissions = SitePermission.GetAll();
            foreach(SitePermission permission in permissions)
            {
                SitePermissions.Add(new SiteUserPermissionVM(permission));
            }

            //Language drop down
            List<Language> languages = Language.GetAll().Where(o => o.ID <= 2).ToList();
            LanguageList = new SelectList(languages, "ID", "LanguageName");
            LanguageID = 1;
        }

    }

    public class SiteUserPermissionVM
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasPermission { get; set; }

        public SiteUserPermissionVM()
        {

        }

        public SiteUserPermissionVM(SitePermission permission)
        {
            Tag = permission.Tag;
            Name = permission.Name;
            Description = permission.Description;
        }
    }
}