using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;
using ProEstimatorData;

namespace ProEstimatorData.DataModel
{
    public class LoginInfo : ProEstEntity
    {

        public int ID { get; private set; }
        public string LoginName { get; set; }
        public string Organization { get; set; }
        public string CompanyName { get; set; }
        public string CompanyType { get; set; }
        public int CompanyOrigin { get; set; }
        public string Password { get; set; }
        public DateTime CreationDate { get; set; }
        public int ContactID { get; set; }
        public string HeaderContact { get; set; }
        public int SalesRepID { get; set; }
        public int ResellerID { get; set; }
        public int NoOfLogins { get; set; }
        public bool DoubtfulAccount { get; set; }
        public bool Disabled { get; set; }
        public bool StaffAccount { get; set; }
        public string RegistrationNumber { get; set; }
        public bool PrintRegistrationNumber { get; set; }
        public bool ShowRepairShopProfiles { get; set; }
        public bool AllowAlternateIdentities { get; set; }
        public bool Appraiser { get; set; }
        public bool ShowLaborTimeWO { get; set; }
        public string LicenseNumber { get; set; }
        public bool PrintLicenseNumber { get; set; }
        public string BarNumber { get; set; }
        public bool PrintBarNumber { get; set; }
        public string FederalTaxID { get; set; }
        public bool PrintFederalTaxID { get; set; }
        public bool UseTaxID { get; set; }
        public bool UseDefaultRateProfile { get; set; }
        public bool UseDefaultPDRRateProfile { get; set; }
        public string LogoImageType { get; set; }
        public bool ProfileLocked { get; set; }
        public bool PartsNow { get; set; }
        public bool OverlapAdmin { get; set; }
        public int LastEstimateNumber { get; set; }
        public int LastWorkOrderNumber { get; set; }
        public DateTime? CarfaxExcludeDate { get; set; }
        public string TechSupportPassword { get; set; }
        public int LanguageID { get; set; }
        public string IntelliPayMerchantKey { get; set; }
        public string IntelliPayAPIKey { get; set; }
        public bool IntelliPayUseCardReader { get; set; }

        public string GetLogoPath()
        {
            if (string.IsNullOrEmpty(LogoImageType))
            {
                return System.Configuration.ConfigurationManager.AppSettings["BaseURL"] + "/content/images/WebEstGlobalSmall.png";
            }
            else
            {
                return System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["BaseURL"], "UserContent", "CompanyLogos", this.ID.ToString() + "." + LogoImageType + "?r=" + DateTime.Now.Millisecond);
            }
        }

        public LoginInfo()
        {
            ID = 0;
            LoginName = "";
            Organization = "";
            CompanyName = "";
            CompanyType = "";
            CompanyOrigin = 255;
            Password = "";
            CreationDate = DateTime.Now;
            ContactID = 0;
            HeaderContact = "";
            SalesRepID = 0;
            ResellerID = 0;
            NoOfLogins = 1;
            DoubtfulAccount = false;
            Disabled = false;
            StaffAccount = false;
            RegistrationNumber = "";
            PrintRegistrationNumber = false;
            ShowRepairShopProfiles = false;
            AllowAlternateIdentities = false;
            Appraiser = false;
            ShowLaborTimeWO = false;
            LicenseNumber = "";
            PrintLicenseNumber = false;
            BarNumber = "";
            PrintBarNumber = false;
            FederalTaxID = "";
            PrintFederalTaxID = false;
            UseTaxID = false;
            UseDefaultRateProfile = false;
            UseDefaultPDRRateProfile = false;
            LogoImageType = "";
            ProfileLocked = false;
            PartsNow = false;
            OverlapAdmin = false;
            LastEstimateNumber = 0;
            LastWorkOrderNumber = 0;
            CarfaxExcludeDate = null;
            TechSupportPassword = "";
            LanguageID = 1;
        }

        public LoginInfo(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginName = InputHelper.GetString(row["LoginName"].ToString());
            Organization = InputHelper.GetString(row["Organization"].ToString());
            CompanyName = InputHelper.GetString(row["CompanyName"].ToString());
            CompanyType = InputHelper.GetString(row["CompanyType"].ToString());
            CompanyOrigin = InputHelper.GetInteger(row["CompanyOrigin"].ToString());
            Password = InputHelper.GetString(row["Password"].ToString());
            CreationDate = InputHelper.GetDateTime(row["CreationDate"].ToString());
            ContactID = InputHelper.GetInteger(row["ContactID"].ToString());
            HeaderContact = InputHelper.GetString(row["HeaderContact"].ToString());
            SalesRepID = InputHelper.GetInteger(row["SalesRepID"].ToString());
            ResellerID = InputHelper.GetInteger(row["ResellerID"].ToString());
            NoOfLogins = InputHelper.GetInteger(row["NoOfLogins"].ToString());
            DoubtfulAccount = InputHelper.GetBoolean(row["DoubtfulAccount"].ToString());
            Disabled = InputHelper.GetBoolean(row["Disabled"].ToString());
            StaffAccount = InputHelper.GetBoolean(row["StaffAccount"].ToString());
            RegistrationNumber = InputHelper.GetString(row["RegistrationNumber"].ToString());
            PrintRegistrationNumber = InputHelper.GetBoolean(row["PrintRegistrationNumber"].ToString());
            ShowRepairShopProfiles = InputHelper.GetBoolean(row["ShowRepairShopProfiles"].ToString());
            AllowAlternateIdentities = InputHelper.GetBoolean(row["AllowAlternateIdentities"].ToString());
            Appraiser = InputHelper.GetBoolean(row["Appraiser"].ToString());
            ShowLaborTimeWO = InputHelper.GetBoolean(row["ShowLaborTimeWO"].ToString());
            LicenseNumber = InputHelper.GetString(row["LicenseNumber"].ToString());
            PrintLicenseNumber = InputHelper.GetBoolean(row["PrintLicenseNumber"].ToString());
            BarNumber = InputHelper.GetString(row["BarNumber"].ToString());
            PrintBarNumber = InputHelper.GetBoolean(row["PrintBarNumber"].ToString());
            FederalTaxID = InputHelper.GetString(row["FederalTaxID"].ToString());
            PrintFederalTaxID = InputHelper.GetBoolean(row["PrintFederalTaxID"].ToString());
            UseTaxID = InputHelper.GetBoolean(row["UseTaxID"].ToString());
            UseDefaultRateProfile = InputHelper.GetBoolean(row["UseDefaultRateProfile"].ToString());
            UseDefaultPDRRateProfile = InputHelper.GetBoolean(row["UseDefaultPDRRateProfile"].ToString());
            LogoImageType = InputHelper.GetString(row["LogoImageType"].ToString());
            ProfileLocked = InputHelper.GetBoolean(row["ProfileLocked"].ToString());
            PartsNow = InputHelper.GetBoolean(row["PartsNow"].ToString());
            OverlapAdmin = InputHelper.GetBoolean(row["OverlapAdmin"].ToString());
            LastEstimateNumber = InputHelper.GetInteger(row["LastEstimateNumber"].ToString());
            LastWorkOrderNumber = InputHelper.GetInteger(row["LastWorkOrderNumber"].ToString());
            CarfaxExcludeDate = InputHelper.GetNullableDateTime(row["CarfaxExcludeDate"].ToString());
            TechSupportPassword = InputHelper.GetString(row["TechSupportPassword"].ToString());
            LanguageID = InputHelper.GetInteger(row["LanguageID"].ToString(), 1);
            IntelliPayMerchantKey = InputHelper.GetString(row["IntelliPayMerchantKey"].ToString());
            IntelliPayAPIKey = InputHelper.GetString(row["IntelliPayAPIKey"].ToString());
            IntelliPayUseCardReader = InputHelper.GetBoolean(row["IntelliPayUseCardReader"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            if (CreationDate == DateTime.MinValue)
            {
                CreationDate = DateTime.Now;
            }

            // Checking null value if it is not first request to create / click on 'Create New Account'
            if (CarfaxExcludeDate != null && CarfaxExcludeDate == DateTime.MinValue)
            {
                CarfaxExcludeDate = DateTime.Now;
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginName", InputHelper.GetString(LoginName)));
            parameters.Add(new SqlParameter("Organization", InputHelper.GetString(Organization)));
            parameters.Add(new SqlParameter("CompanyName", InputHelper.GetString(CompanyName)));
            parameters.Add(new SqlParameter("CompanyType", InputHelper.GetString(CompanyType)));
            parameters.Add(new SqlParameter("CompanyOrigin", CompanyOrigin));
            parameters.Add(new SqlParameter("Password", InputHelper.GetString(Password)));
            parameters.Add(new SqlParameter("CreationDate", CreationDate));
            parameters.Add(new SqlParameter("ContactID", ContactID));
            parameters.Add(new SqlParameter("HeaderContact", InputHelper.GetString(HeaderContact)));
            parameters.Add(new SqlParameter("SalesRepID", SalesRepID));
            parameters.Add(new SqlParameter("ResellerID", ResellerID));
            parameters.Add(new SqlParameter("NoOfLogins", NoOfLogins));
            parameters.Add(new SqlParameter("DoubtfulAccount", DoubtfulAccount));
            parameters.Add(new SqlParameter("Disabled", Disabled));
            parameters.Add(new SqlParameter("StaffAccount", StaffAccount));
            parameters.Add(new SqlParameter("RegistrationNumber", InputHelper.GetString(RegistrationNumber)));
            parameters.Add(new SqlParameter("PrintRegistrationNumber", PrintRegistrationNumber));
            parameters.Add(new SqlParameter("ShowRepairShopProfiles", ShowRepairShopProfiles));
            parameters.Add(new SqlParameter("AllowAlternateIdentities", AllowAlternateIdentities));
            parameters.Add(new SqlParameter("Appraiser", Appraiser));
            parameters.Add(new SqlParameter("ShowLaborTimeWO", ShowLaborTimeWO));
            parameters.Add(new SqlParameter("LicenseNumber", InputHelper.GetString(LicenseNumber)));
            parameters.Add(new SqlParameter("PrintLicenseNumber", PrintLicenseNumber));
            parameters.Add(new SqlParameter("BarNumber", InputHelper.GetString(BarNumber)));
            parameters.Add(new SqlParameter("PrintBarNumber", PrintBarNumber));
            parameters.Add(new SqlParameter("FederalTaxID", InputHelper.GetString(FederalTaxID)));
            parameters.Add(new SqlParameter("PrintFederalTaxID", PrintFederalTaxID));
            parameters.Add(new SqlParameter("UseTaxID", UseTaxID));
            parameters.Add(new SqlParameter("UseDefaultRateProfile", UseDefaultRateProfile));
            parameters.Add(new SqlParameter("UseDefaultPDRRateProfile", UseDefaultPDRRateProfile));
            parameters.Add(new SqlParameter("LogoImageType", InputHelper.GetString(LogoImageType)));
            parameters.Add(new SqlParameter("ProfileLocked", ProfileLocked));
            parameters.Add(new SqlParameter("PartsNow", PartsNow));
            parameters.Add(new SqlParameter("OverlapAdmin", OverlapAdmin));
            parameters.Add(new SqlParameter("LastEstimateNumber", LastEstimateNumber));
            parameters.Add(new SqlParameter("LastWorkOrderNumber", LastWorkOrderNumber));
            parameters.Add(new SqlParameter("CarfaxExcludeDate", CarfaxExcludeDate));
            parameters.Add(new SqlParameter("TechSupportPassword", InputHelper.GetString(TechSupportPassword)));
            parameters.Add(new SqlParameter("LanguageID", LanguageID));
            parameters.Add(new SqlParameter("IntelliPayMerchantKey", IntelliPayMerchantKey));
            parameters.Add(new SqlParameter("IntelliPayAPIKey", IntelliPayAPIKey));
            parameters.Add(new SqlParameter("IntelliPayUseCardReader", IntelliPayUseCardReader));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Logins_Update", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "LoginInfo", ID, ID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static LoginInfo GetByID(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("Logins_GetByID", new SqlParameter("ID", id));

            if (result.Success)
            {
                return new LoginInfo(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static LoginInfo GetByName(string loginName, string organization)
        {
            if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(organization))
            {
                return null;
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginName", loginName));
            parameters.Add(new SqlParameter("Organization", organization));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("Logins_GetByName", parameters);

            if (result.Success)
            {
                return new LoginInfo(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<LoginInfo> GetByEmailAddress(string emailAddress)
        {
            List<LoginInfo> logins = new List<LoginInfo>();

            if (!string.IsNullOrEmpty(emailAddress))
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("EmailAddress", emailAddress));

                DBAccess db = new DBAccess();
                DBAccessTableResult result = db.ExecuteWithTable("Logins_GetByEmail", parameters);

                foreach (DataRow row in result.DataTable.Rows)
                {
                    logins.Add(new LoginInfo(row));
                }
            }

            return logins;
        }

        public static List<LoginInfo> GetByOrganization(string organization)
        {
            List<LoginInfo> logins = new List<LoginInfo>();

            if (!string.IsNullOrEmpty(organization))
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@Organization", organization));

                DBAccess db = new DBAccess();
                DBAccessTableResult result = db.ExecuteWithTable("Logins_GetByOrganization", parameters);

                foreach (DataRow row in result.DataTable.Rows)
                {
                    logins.Add(new LoginInfo(row));
                }
            }

            return logins;
        }

        public static List<LoginInfo> GetByCredentials(string loginName, string organization, string password)
        {
            if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@LoginName", loginName));
            parameters.Add(new SqlParameter("@Organization", organization));
            parameters.Add(new SqlParameter("@Password", password));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("Logins_GetByCredentials", parameters);

            if (result.Success)
            {
                List<LoginInfo> results = new List<LoginInfo>();
                foreach (DataRow row in result.DataTable.Rows)
                {
                    results.Add(new LoginInfo(row));
                }

                return results;
            }

            return null;
        }

        /// <summary>
        /// True if the user has fully moved over from Web-Est to ProEstimator
        /// </summary>
        /// <returns></returns>
        public static bool IsWebEstConversionComplete(int loginID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult table = db.ExecuteWithTable("LoginsConversionCompleteGet", new SqlParameter("LoginID", loginID));
            return table.Success;
        }
    }
}
