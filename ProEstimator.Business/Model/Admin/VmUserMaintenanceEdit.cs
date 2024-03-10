using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Model.Admin
{
    public class VmUserMaintenanceEdit
    {
        public int UserId { get; set; }
        public bool AccountDisabled { get; set; }
        public bool DoubtfulAccount { get; set; }
        public bool StaffAccount { get; set; }
        public bool AppraiserAccount { get; set; }
        public string LoginName { get; set; }
        public string Organization { get; set; }
        public string LoginOrg { get; set; }
        public int LoginId { get; set; }
        public int CompanyOrigin { get; set; }
        public string ExpireDate { get; set; }
        public string ExpireDateDetails { get; set; }
        public int NumberOfLogins { get; set; }
        public string ContractId { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string JobTitle { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber1 { get; set; }
        public string PhoneNumber2 { get; set; }
        public string FaxNumber { get; set; }
        public VmOption PhoneNumber1Type { get; set; }
        public VmOption PhoneNumber2Type { get; set; }
        public string TechSupportPassword { get; set; }
        public int SalesRepId { get; set; }
        public int? ResellerId { get; set; }
        public bool PasswordExpired { get; set; }
        public int ContactId { get; set; }

        public int ActiveContractID { get; set; }
        public int ActiveTrialID { get; set; }

        // The total owed by the user, formatted as currency for display
        public string TotalAmountDue { get; set; }

        public VmUserMaintenanceEdit(LoginInfo loginInfo)
        {
            UserId = loginInfo.ID;
            AccountDisabled = loginInfo.Disabled;
            DoubtfulAccount = loginInfo.DoubtfulAccount;
            StaffAccount = loginInfo.StaffAccount;
            AppraiserAccount = loginInfo.StaffAccount;
            LoginName = loginInfo.LoginName;
            Organization = loginInfo.CompanyName;
            LoginOrg = loginInfo.Organization;
            LoginId = loginInfo.ID;
            CompanyOrigin = loginInfo.CompanyOrigin;

            Contact contact = Contact.GetContact(loginInfo.ContactID);
            FirstName = contact.FirstName;
            LastName = contact.LastName;
            JobTitle = contact.Title;
            EmailAddress = contact.Email;
            PhoneNumber1 = contact.Phone;
            PhoneNumber2 = contact.Phone2;
            FaxNumber = contact.Fax;

            try
            {
                PhoneNumber1Type = new VmOption()
                {
                    Type = contact.PhoneNumberType1.ToString()
                };
                PhoneNumber2Type = new VmOption()
                {
                    Type = contact.PhoneNumberType1.ToString()
                };
            }
            catch { }

            TechSupportPassword = loginInfo.TechSupportPassword;
            SalesRepId = loginInfo.SalesRepID;
            ResellerId = loginInfo.ResellerID;

            Password = loginInfo.Password;
        }

        //public VmUserMaintenanceEdit ToModel(DataRow row)
        //{
        //    var model = new VmUserMaintenanceEdit();
        //    model.UserId = (int)row["id"];
        //    model.AccountDisabled = (bool)row["Disabled"];
        //    model.DoubtfulAccount = (bool)row["DoubtfulAccount"];
        //    model.StaffAccount = (bool)row["StaffAccount"];
        //    model.AppraiserAccount = (bool)row["StaffAccount"];
        //    model.LoginName = row["loginname"].SafeString();
        //    model.Organization = row["CompanyName"].SafeString();
        //    model.LoginOrg = row["Organization"].SafeString();
        //    model.LoginId = (int)row["id"];
        //    model.CompanyOrigin = row["CompanyOrigin"].ByteToInt();
        //    model.FirstName = row["FirstName"].SafeString();
        //    model.MiddleName = row["MiddleName"].SafeString();
        //    model.LastName = row["LastName"].SafeString();
        //    model.JobTitle = row["JobTitle"].SafeString();
        //    model.EmailAddress = row["EmailAddress"].SafeString();
        //    model.PhoneNumber1 = row["Phone1"].SafeString();
        //    model.PhoneNumber2 = row["Phone2"].SafeString();
        //    model.FaxNumber = row["FaxNumber"].SafeString();
        //    model.PhoneNumber1Type = new VmOption()
        //    {
        //        Type = row["PhoneNumberType1"].SafeString()
        //    };
        //    model.PhoneNumber2Type = new VmOption()
        //    {
        //        Type = row["PhoneNumberType2"].SafeString()
        //    };
        //    model.TechSupportPassword = row["TechSupportPassword"].SafeString();
        //    model.SalesRepId = (int)row["SalesRepID"];
        //    model.PasswordExpired = (bool)row["PasswordExpired"];
        //    model.ResellerId = row["ResellerId"].SafeInt();

        //    model.Password = row["Password"].SafeString();
        //    if (string.IsNullOrEmpty(model.Password))
        //    {
        //        model.Password = row["encPassword"].SafeString();
        //    }

        //    return model;
        //}
    }
}
