using System;
using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmOrganizationEdit : IModelMap<VmOrganizationEdit>, ITrackable<VmOrganizationEdit>
    {
        public int ID { get; set; }
        public string CustomerNumber { get; set; }
        public bool? PaidContract { get; set; }
        public bool? Disabled { get; set; }
        public string ExpireDate { get; set; }
        public string GraphicsExpireDate { get; set; }
        public string ManualEntryExpireDate { get; set; }
        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone1 { get; set; }
        public string Phone1Type { get; set; }
        public string Phone2 { get; set; }
        public string Phone2Type { get; set; }
        public string Phone3 { get; set; }
        public string FaxNumber { get; set; }
        public string Phone3Type { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CompanyType { get; set; }
        public int? EstimateId { get; set; }
        public string FederalTaxId { get; set; }
        public string RegistrationId { get; set; }
        public string LicenseId { get; set; }
        public string BarNumber { get; set; }
        public bool? ShowRepair { get; set; }
        public bool? AllowAlternate { get; set; }
        public bool? AllowEditing { get; set; }
        public bool? ShowAppraiser { get; set; }
        public bool? PasswordRequired { get; set; }
        public bool? ProfileLocked { get; set; }
        public string DelPassword { get; set; }

        public VmOrganizationEdit ToModel(DataRow row)
        {
            var model = new VmOrganizationEdit();
            model.ID = (int)row["id"];
            model.CustomerNumber = row["CustomerNumber"].SafeString();
            model.PaidContract = row["PaidContract"].SafeBool();
            model.Disabled = row["Disabled"].SafeBool();
            model.ExpireDate = row["ExpireDate"].SafeDate();
            model.GraphicsExpireDate = row["GraphicsExpireDate"].SafeDate();
            model.ManualEntryExpireDate = row["ManualExpireDate"].SafeDate();
            model.CompanyName = row["CompanyName"].SafeString();
            model.Address1 = row["AddressLine1"].SafeString();
            model.Address2 = row["AddressLine2"].SafeString();
            model.City = row["City"].SafeString();
            model.State = row["State"].SafeString();
            model.Zip = row["Zip"].SafeString();
            model.Phone1 = row["Phone1"].SafeString();
            model.Phone1Type = row["PhoneCode1"].SafeString();
            model.Phone2 = row["Phone2"].SafeString();
            model.Phone2Type = row["PhoneCode2"].SafeString();
            model.Phone3 = row["Phone3"].SafeString();
            model.FaxNumber = row["FaxNumber"].SafeString();
            model.Phone3Type = row["PhoneCode3"].SafeString();
            model.FirstName = row["EstimatorFirstName"].SafeString();
            model.MiddleName = row["EstimatorMiddleName"].SafeString();
            model.LastName = row["EstimatorLastName"].SafeString();
            model.Email = row["EmailAddress"].SafeString();
            model.CompanyType = row["CompanyType"].SafeString();
            model.EstimateId = row["LastEstimateNumber"].SafeInt();
            model.FederalTaxId = row["FederalTaxId"].SafeString();
            model.RegistrationId = row["RegistrationNumber"].SafeString();
            model.LicenseId = row["LicenseNumber"].SafeString();
            model.BarNumber = row["BarNumber"].SafeString();
            model.ShowRepair = row["ShowRepairShopProfiles"].SafeBool();
            model.AllowAlternate = row["AllowAlternateIdentities"].SafeBool();
            model.AllowEditing = row["AllowRWofInspAssignDates"].SafeBool();
            model.ShowAppraiser = row["Appraiser"].SafeBool();
            model.PasswordRequired = row["DelPWRequired"].SafeBool();
            model.ProfileLocked = row["ProfileLocked"].SafeBool();
            model.DelPassword = row["DelPW"].SafeString();

            return model;
        }

        public string ToTrackable()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public string Serialized { get; set; }


        public VmOrganizationEdit FromTrackable(string item)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<VmOrganizationEdit>(item);
        }

        public bool CheckForNulls(VmOrganizationEdit obj)
        {
            var result = false;
            if (!string.IsNullOrEmpty(obj.FirstName) && string.IsNullOrEmpty(FirstName) &&
                !string.IsNullOrEmpty(obj.LastName) && string.IsNullOrEmpty(LastName) &&
                !string.IsNullOrEmpty(obj.Email) && string.IsNullOrEmpty(Email))
            {
                result = true;
            }
            else {
                result = false;
            }

            return result;
        }
    }
}
