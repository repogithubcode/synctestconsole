using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.IO;

using ProEstimatorData;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Extension;
using ProEstimator.Business.Logic.Admin;
using ProEstimator.Business.Model.Admin;
using ProEstimator.Business.Model.Admin.Reports;
using ProEstimator.Business.Type;
using ProEstimator.Business.Structure;
using ProEstimator.Business.Payments;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Logic
{
    public class AdminService
    {
        private readonly DBAccess _data;
        private readonly TextMessageService _smsService;
        private readonly NewUserSignupService _signUpService;
        private readonly string SERVER = ConfigurationManager.AppSettings["server"];

        public AdminService()
        {
            _data = new DBAccess();
            _smsService = new TextMessageService();
            _signUpService = new NewUserSignupService();
        }

        public DataSet GetUserInfo(string loginId)
        {
            var result = new DataSet();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginsID", loginId));

            var item = _data.ExecuteWithTable(AdminConstants.GET_USER_INFO, parameters);
            if (item != null && item.DataTable != null)
            {
                item.DataTable.TableName = AdminConstants.NO_OF_LOGINS_TABLE;
                result.Tables.Add(item.DataTable);
            }

            return result;
        }

        public DataSet GetNumberOfLoginsDS(string loginId)
        {
            var result = new DataSet();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginId", loginId));

            var item = _data.ExecuteWithTable(AdminConstants.GET_NUMBER_OF_LOGINS, parameters);
            if (item != null && item.DataTable != null)
            {
                item.DataTable.TableName = AdminConstants.USER_INFO_TABLE;
                result.Tables.Add(item.DataTable);
            }

            return result;
        }

        public int GetNumberOfLogins(string loginId)
        {
            var result = 0;
            var ds = GetNumberOfLoginsDS(loginId);
            if (ds != null)
            {
                result = Convert.ToInt32(ds.Tables[0].Rows[0].ItemArray[0].ToString());
            }

            return result;
        }

        public string GetRandomPassword()
        {
            string result = null;
            while (result == null)
            {
                const string sample = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
                var list = new List<string>();
                list.AddRange(sample.Select(item => item.ToString()));
                var random = list.OrderBy(item => Guid.NewGuid()).Take(10);
                var enumerable = random as IList<string> ?? random.ToList();

                if (PasswordRules(enumerable))
                {
                    result = String.Join(string.Empty, enumerable);
                }
            }

            return result;
        }

        private static bool PasswordRules(IList<string> enumerable)
        {
            int myInt;
            return (enumerable.Any(x => Char.IsUpper(Convert.ToChar(x))) &&
                    enumerable.Any(x => int.TryParse(x, out myInt)) ||
                   (enumerable.Any(x => Char.IsUpper(Convert.ToChar(x))) &&
                    enumerable.Any(x => !Char.IsLetterOrDigit(Convert.ToChar(x)))) ||
                   (enumerable.Any(x => !Char.IsLetterOrDigit(Convert.ToChar(x))) &&
                    enumerable.Any(x => int.TryParse(x, out myInt)))) &&
                    enumerable.Any(x => Char.IsLower(Convert.ToChar(x)));
        }

        public string Encrypt(int item)
        {
            var result = string.Empty;
            result = Encryptor.Encrypt(item.ToString());
            result = HttpUtility.UrlEncode(result);

            return result;
        }

        public VmCreateOrg GetOrganization(string organization)
        {
            VmCreateOrg result = null;
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("companyName", organization ?? string.Empty));

            var item = _data.ExecuteWithTable(AdminConstants.GET_ORG_SINGLE, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                return (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmCreateOrg>()).FirstOrDefault();
            }

            return result;
        }

        public List<VmOrganizationSearchResult> SearchOrganizations(string organization)
        {
            var result = new List<VmOrganizationSearchResult>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("SearchName", organization ?? string.Empty));

            var item = _data.ExecuteWithTable(AdminConstants.ORG_SEARCH, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                return (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmOrganizationSearchResult>()).ToList();
            }

            return result;
        }

        public VmStandardResult<VmCreateOrg> CreateOrganization(string organization, string loginId, string expireDate)
        {
            var result = new VmStandardResult<VmCreateOrg>();
            VmCreateOrg model = null;
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginsID", loginId));
            parameters.Add(new SqlParameter("Organization", organization));
            parameters.Add(new SqlParameter("OrgID", -1));

            //var prelim = GetOrganization(organization);
            //if (prelim != null)
            //{
            //    result.Status = false;
            //    result.Payload = prelim;
            //    result.Message =
            //        string.Format(
            //            "The organization has not been created due to an error. There is an existing organization with the name {1} and ID {0}",
            //            prelim.OrgId, prelim.CompanyName);

            //    return result;
            //}

            var item = _data.ExecuteWithTable(AdminConstants.ORG_CREATE, parameters);
            UpdateOrganizationContract(expireDate, loginId);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                model = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmCreateOrg>()).FirstOrDefault();
            }

            if (model != null)
            {
                // The org was created, set up a trial contract
                ContractManager.CreateTrial(InputHelper.GetInteger(loginId), DateTime.Now, DateTime.Now.AddDays(14), false, false, false, true, true, true, false,false);

                result.Status = true;
                result.Payload = model;
                result.Message = "The organization has been created.  You should complete the information for it on the Org Maintenance screen.";
            }
            else
            {
                result.Status = false;
                result.Message = "The organization was not created due to an error.  Please contact the programmer.";
            }

            return result;
        }

        public List<VmSalesRep> FilterSalesReps(bool v1, bool v2, List<VmSalesRep> input)
        {
            return input.Where(x => x.Active && x.IsSalesRep).OrderBy(r => r.FirstName).ToList();
        }

        private string GetOrgMaintLink(decimal orgId)
        {
            return "<a href=\"/#/salesRep/org-maitenance/" + orgId + "\">Edit Org</a>";
        }



        public List<VmCustomerSearch> SearchCustomer(string loginId, string loginName, string companyOrg,
            string firstName, string lastName)
        {
            var dataSet = new DataSet();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", string.IsNullOrEmpty(loginId) ? null : loginId));
            parameters.Add(new SqlParameter("LoginName", loginName));
            parameters.Add(new SqlParameter("CompanyName", companyOrg));
            parameters.Add(new SqlParameter("FirstName", firstName));
            parameters.Add(new SqlParameter("LastName", lastName));

            var item = _data.ExecuteWithTable(AdminConstants.GET_CUSTOMER_CONTACTS, parameters);
            if (item != null && item.DataTable != null)
            {
                dataSet.Tables.Add(item.DataTable);
            }

            return (from DataRow row in dataSet.Tables[0].Rows select row.ToModel<VmCustomerSearch>()).ToList();
        }

        private string FormatExpireDate(string expireDate)
        {
            var result = string.Empty;
            DateTime myDate;
            if (DateTime.TryParse(expireDate, out myDate))
            {
                result =
                    new DateTime(myDate.Year, myDate.Month, myDate.Day, 23, 59, 59).ToString(
                        CultureInfo.InvariantCulture);
            }

            return result;
        }

        private VmUserMaintenanceEdit StageDataForNewOrg(VmUserMaintenanceEdit model)
        {
            if (string.IsNullOrEmpty(model.LoginName))
            {
                model.LoginName = string.Empty;
            }

            return model;
        }

        public List<VmOrganizationEdit> GetOrganizationModel(string orgId)
        {
            var dataSet = new DataSet();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("orgId", orgId));

            var item = _data.ExecuteWithTable(AdminConstants.GET_ORG, parameters);
            if (item != null && item.DataTable != null)
            {
                item.DataTable.TableName = AdminConstants.NO_OF_LOGINS_TABLE;
                dataSet.Tables.Add(item.DataTable);
            }

            var result = (from DataRow row in dataSet.Tables[0].Rows select row.ToModel<VmOrganizationEdit>()).ToList();
            foreach (var x in result)
            {
                x.Serialized = x.ToTrackable();
            }

            return result;
        }

        private string SubtractHours(string date, int hours)
        {
            var result = string.Empty;
            DateTime myDate;
            if (DateTime.TryParse(date, out myDate))
            {
                result = myDate.Subtract(new TimeSpan(hours, 0, 0)).ToString(CultureInfo.InvariantCulture);
            }

            return result;
        }

        public bool SaveForecast(int id, int month, int year, int forecast)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@SalesRepId", id));
            parameters.Add(new SqlParameter("@month", month));
            parameters.Add(new SqlParameter("@year", year));
            parameters.Add(new SqlParameter("@forecast", forecast));
            _data.ExecuteNonQuery(AdminConstants.SaveForecast, parameters);
            return true;
        }

        public void SaveRenewalBonuses(RenewalReportBonus item)
        {
            var bonus = GetRenewalBonuses(item.SalesRepId, item.BonusMonth, item.BonusYear);
            var ds = new DataSet();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@SalesRepId", item.SalesRepId));
            parameters.Add(new SqlParameter("@BonusMonth", item.BonusMonth));
            parameters.Add(new SqlParameter("@BonusYear", item.BonusYear));
            parameters.Add(new SqlParameter("@RenewalGoal1Yr", item.RenewalGoal1Yr));
            parameters.Add(new SqlParameter("@RenewalGoal2Yr", item.RenewalGoal2Yr));
            parameters.Add(new SqlParameter("@SalesGoal", item.SalesGoal));
            parameters.Add(new SqlParameter("@SalesBonus100", item.SalesBonus100));
            parameters.Add(new SqlParameter("@SalesBonus110", item.SalesBonus110));
            parameters.Add(new SqlParameter("@SalesBonus120", item.SalesBonus120));
            parameters.Add(new SqlParameter("@SalesBonus130", item.SalesBonus130));
            parameters.Add(new SqlParameter("@RenewalBonus1Yr100", item.RenewalBonus1Yr100));
            parameters.Add(new SqlParameter("@RenewalBonus1Yr110", item.RenewalBonus1Yr110));
            parameters.Add(new SqlParameter("@RenewalBonus120", item.RenewalBonus120));
            parameters.Add(new SqlParameter("@RenewalBonus130", item.RenewalBonus130));
            parameters.Add(new SqlParameter("@ForeCast", item.Forecast));
            // insert
            if (bonus == null)
            {
                _data.ExecuteNonQuery(AdminConstants.Insertrenewalgoal, parameters);
            }
            else // update
            {
                _data.ExecuteNonQuery(AdminConstants.Updaterenewalgoal, parameters);
            }
        }

        public RenewalReportBonus GetRenewalBonuses(int id, int month, int year)
        {
            DBAccessTableResult item;
            var ds = new DataSet();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@BonusMonth", month));
            parameters.Add(new SqlParameter("@BonusYear", year));
            if (id != -1)
            {
                parameters.Add(new SqlParameter("@SalesRepId", id));
                item = _data.ExecuteWithTable(AdminConstants.Selectrenewalgoal, parameters);
            }
            else
            {
                item = _data.ExecuteWithTable(AdminConstants.SelectrenewalgoalForActive, parameters);
            }
            if (item != null && item.DataTable != null)
            {
                item.DataTable.TableName = AdminConstants.RenewalReportSettingsTable;
                ds.Tables.Add(item.DataTable);
            }

            var result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<RenewalReportBonus>()).FirstOrDefault();

            if (result != null)
            {
                result.ActualSales = GetActualSales(id, month, year);
            }

            return result;
        }

        private int GetActualSales(int id, int month, int year)
        {
            DBAccessIntResult item;
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@month", month));
            parameters.Add(new SqlParameter("@year", year));
            if (id != -1)
            {
                parameters.Add(new SqlParameter("@SalesRepId", id));
                item = _data.ExecuteWithIntReturn(AdminConstants.SelectTotalSold, parameters);
            }
            else
            {
                item = _data.ExecuteWithIntReturn(AdminConstants.SelectTotalSoldForActive, parameters);
            }

            return item.Value;
        }

        public void SaveRenewalDisplaySettings(int id, string key)
        {
            var settings = GetRenewalDisplaySettings(id);
            // insert
            if (settings == null)
            {
                var ds = new DataSet();
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@SalesRepId", id));
                parameters.Add(new SqlParameter("@SettingsKey", key));
                _data.ExecuteNonQuery(AdminConstants.InsertRenewalReportSettings, parameters);
            }
            else // update
            {
                var ds = new DataSet();
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@SalesRepId", id));
                parameters.Add(new SqlParameter("@SettingsKey", key));
                _data.ExecuteNonQuery(AdminConstants.UpdateRenewalReportSettings, parameters);
            }
        }

        public RenewalReportDisplaySettings GetRenewalDisplaySettings(int id)
        {
            var ds = new DataSet();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@SalesRepId", id));
            var item = _data.ExecuteWithTable(AdminConstants.SelectRenewalReportSettings, parameters);
            if (item != null && item.DataTable != null)
            {
                item.DataTable.TableName = AdminConstants.RenewalReportSettingsTable;
                ds.Tables.Add(item.DataTable);
            }

            var result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<RenewalReportDisplaySettings>()).FirstOrDefault();

            return result;
        }

        public string UpdateOrganizationContract(string expireDate, string loginId)
        {
            var result = string.Empty;
            // CONTRACTTODO
            //var parameters = new List<SqlParameter>();
            //parameters.Add(new SqlParameter("expireDate",
            //    string.IsNullOrEmpty(expireDate) ? null : SubtractHours(expireDate, 1)));
            //parameters.Add(new SqlParameter("loginId", loginId));

            //var item = _data.ExecuteWithTable(AdminConstants.UPDATE_CONTRACT, parameters);
            //if (item != null && item.DataTable != null)
            //{
            //    result = (string)item.DataTable.Rows[0][0];
            //}

            return result;
        }

        public string GetPromoById(int id)
        {
            var result = string.Empty;
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("PromoID", id));

            var item = _data.ExecuteWithTable(AdminConstants.GET_PROMO_BY_ID, parameters);
            if (item != null && item.DataTable != null)
            {
                result = (string)item.DataTable.Rows[0][0];
            }

            return result;
        }

        public VmInvoice GetInvoice(string loginId, string contractId)
        {
            var result = new VmInvoice();
            var dataSet = new DataSet();
            var dataSet1 = new DataSet();
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(loginId))
            {
                parameters.Add(new SqlParameter("LoginID", loginId));
            }
            if (!string.IsNullOrEmpty(contractId))
            {
                parameters.Add(new SqlParameter("ContractID", contractId));
            }

            var item = _data.ExecuteWithTable(AdminConstants.NEW_GET_CONTRACT, parameters);
            if (item != null && item.DataTable != null)
            {
                item.DataTable.TableName = AdminConstants.NO_OF_LOGINS_TABLE;
                dataSet.Tables.Add(item.DataTable);
            }

            result = (from DataRow row in dataSet.Tables[0].Rows select row.ToModel<VmInvoice>()).FirstOrDefault();

            if (result != null)
            {
                result.SelectedContractId = contractId;

                List<Contract> allContracts = Contract.GetAllForLogin(InputHelper.GetInteger(loginId));

                //CONTRACTTODO
                //result.HasFrameDataContract = ContractManager.GetActiveContractByType(allContracts, 2) != null;
                //result.HasEMSContract = ContractManager.GetActiveContractByType(allContracts, 5) != null;
                //result.HasPDRContract = ContractManager.GetActiveContractByType(allContracts, 7) != null;
                //result.HasMultiUserContract = ContractManager.GetActiveContractByType(allContracts, 8) != null;

                if (result.PriceLevel >= 0)
                {
                    result.Details = GetInvoiceDetails(result.ContractId, null, false, AddonType.FrameData);
                    result.Details.ForEach(d =>
                    {
                        if (!string.IsNullOrEmpty(d.Notes) && d.Notes.Contains("Promo"))
                        {
                            result.Promos.Add(new VmPromo() { Id = d.Notes.Split(',')[0].Split(':')[1].Trim(), InvoiceId = d.Id });
                        }
                    });
                }

                if (result.PromoID != null && result.PromoID != 0)
                {
                    if (result.Promos.Count == 0)
                    {
                        var promoCode = GetPromoById((int)result.PromoID);
                        int myPromoCode;
                        //if (int.TryParse(promoCode, out myPromoCode))
                        //{
                        //    result.Promos.Add(new VmPromo() { Id = myPromoCode, InvoiceId = result.Details.FirstOrDefault().Id });
                        //}

                        result.Promos.Add(new VmPromo() { Id = promoCode, InvoiceId = result.Details.FirstOrDefault().Id });
                    }
                }

                result.Logins = GetLogins(result.ContractId);
                result.Contracts = GetFrameDataContracts(loginId);
                result.FrameDataDetails = GetAddonDetails(result, loginId, AddonType.FrameData);
                result.Ems = GetAddonDetails(result, loginId, AddonType.Ems);
                result.Custom = GetAddonDetails(result, loginId, AddonType.Custom);
                result.PDR = GetAddonDetails(result, loginId, AddonType.PDR);
                result.MultiUser = GetAddonDetails(result, loginId, AddonType.MultiUser);
            }

            // get stripe record
            parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@loginId", loginId));
            var stripe = _data.ExecuteWithTable(AdminConstants.GetStripeAuto, parameters);
            if (stripe != null && stripe.DataTable != null)
            {
                dataSet1.Tables.Add(stripe.DataTable);
            }

            var vmStripe = (from DataRow row in dataSet1.Tables[0].Rows select row.ToModel<VmStripe>()).FirstOrDefault();
            if (vmStripe != null && vmStripe.AutoPay != null && result != null)
            {
                result.AutoPay = (bool)vmStripe.AutoPay;
            }
            else // if no stripe record exists, set autopay to false in the UI
            {
                result.AutoPay = false;
            }

            return result;
        }

        private VmAddOnResult GetAddonDetails(VmInvoice item, string loginId, AddonType type)
        {
            var result = new VmAddOnResult();
            var prelim = new VmAddOnResult();
            foreach (var contract in item.Contracts.Where(contract => contract.NewestContract))
            {
                prelim.NewestContractId = contract.ContractId;
                prelim.EffectiveDate = contract.EffectiveDate;

                if (contract.TrialContract)
                {
                    prelim.Trial = true;
                }
            }

            if (type == AddonType.Ems && item.ContractId != null)
            {
                result = item.ContractId != prelim.NewestContractId
                    ? GetContract_AddOn_Result(5, prelim.NewestContractId, 0, loginId)
                    : GetContract_AddOn_Result(5, (int)item.ContractId, 0, loginId);
            }
            else if (type == AddonType.FrameData && item.ContractId != null)
            {
                result = item.ContractId != prelim.NewestContractId
                    ? GetContract_AddOn_Result(2, prelim.NewestContractId, 0, loginId)
                    : GetContract_AddOn_Result(2, (int)item.ContractId, 0, loginId);
            }
            else if (type == AddonType.PDR && item.ContractId != null)
            {
                result = item.ContractId != prelim.NewestContractId
                    ? GetContract_AddOn_Result(7, prelim.NewestContractId, 0, loginId)
                    : GetContract_AddOn_Result(7, (int)item.ContractId, 0, loginId);
            }
            else if (type == AddonType.MultiUser && item.ContractId != null)
            {
                result = item.ContractId != prelim.NewestContractId
                    ? GetContract_AddOn_Result(8, prelim.NewestContractId, 0, loginId)
                    : GetContract_AddOn_Result(8, (int)item.ContractId, 0, loginId);
            }

            if (result != null &&
                result.ContractActive != null &&
                type != AddonType.Custom &&
                (result.AllowReactivation || (!result.AllowReactivation && (bool)result.ContractActive)))
            {
                result.NewestContractId = prelim.NewestContractId;
                result.EffectiveDate = prelim.EffectiveDate;
                result.Trial = prelim.Trial;

                decimal myNextInvoiceAmount;
                if (decimal.TryParse(result.NextInvoiceAmount, out myNextInvoiceAmount) && myNextInvoiceAmount <= 0)
                {
                    result.NextInvoiceAmount = "Paid";
                    result.NextInvoiceDueDate = "Paid";
                }

                if (type == AddonType.Ems)
                {
                    result.Details = GetInvoiceDetails(result.ContractId, null, false, AddonType.Ems);
                }
                else if (type == AddonType.FrameData)
                {
                    result.Details = GetInvoiceDetails(result.ContractId, null, false, AddonType.FrameData);
                }
                else if (type == AddonType.PDR)
                {
                    result.Details = GetInvoiceDetails(result.ContractId, null, false, AddonType.PDR);
                }
                else if (type == AddonType.MultiUser)
                {
                    result.Details = GetInvoiceDetails(result.ContractId, null, false, AddonType.MultiUser);
                }

                result.DisplayPayAll = result.Details.Count > 0;

            }
            else
            {
                result = new VmAddOnResult { Details = new List<VmInvoiceRow>() };
            }

            if (type == AddonType.Custom)
            {
                result.Details = GetInvoiceDetails(null, loginId, false, AddonType.Custom);
                result.DisplayPayAll = result.Details.Count > 0 && result.Details.Any(detail => detail.Status == false);
            }

            return result;
        }

        private List<VmFrameDataContract> GetFrameDataContracts(string loginId)
        {
            var result = new List<VmFrameDataContract>();
            // CONTRACTTODO
            //var parameters = new List<SqlParameter>();
            //parameters.Add(new SqlParameter("LoginID", loginId));

            //var item = _data.ExecuteWithTable(AdminConstants.GetContracts, parameters);
            //if (item != null && item.DataTable != null)
            //{
            //    var ds = new DataSet();
            //    ds.Tables.Add(item.DataTable);
            //    result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmFrameDataContract>()).ToList();
            //}

            return result;
        }

        private List<VmContractLogin> GetLogins(int? contractLoginId)
        {
            // CONTRACTTODO
            //var parameters = new List<SqlParameter>();
            //var dataSet = new DataSet();
            //parameters.Add(new SqlParameter("ContractID", contractLoginId));

            //var item = _data.ExecuteWithTable(AdminConstants.GetContractLogins, parameters);
            //if (item != null && item.DataTable != null)
            //{
            //    item.DataTable.TableName = AdminConstants.NO_OF_LOGINS_TABLE;
            //    dataSet.Tables.Add(item.DataTable);
            //}

            //return (from DataRow row in dataSet.Tables[0].Rows select row.ToModel<VmContractLogin>()).ToList();
            return new List<VmContractLogin>();
        }

        private List<VmInvoiceRow> GetInvoiceDetails(int? contractId, string loginId, bool includeOnlyActiveContractInvoices, AddonType type)
        {
            var parameters = new List<SqlParameter>();
            var dataSet = new DataSet();
            if (type == AddonType.Custom)
            {
                parameters.Add(new SqlParameter("LoginID", loginId));
                parameters.Add(new SqlParameter("InvoiceTypeID", 3));
            }
            else
            {
                parameters.Add(new SqlParameter("ContractID", contractId));
                parameters.Add(new SqlParameter("IncludeOnlyActiveContractInvoices", includeOnlyActiveContractInvoices));
            }

            //CONTRACTTODO 
            //var item = _data.ExecuteWithTable(AdminConstants.GetInvoices, parameters);
            //if (item != null && item.DataTable != null)
            //{
            //    item.DataTable.TableName = AdminConstants.NO_OF_LOGINS_TABLE;
            //    dataSet.Tables.Add(item.DataTable);
            //}

            return (from DataRow row in dataSet.Tables[0].Rows select row.ToModel<VmInvoiceRow>()).ToList();
        }

        public void PayAllInvoices(bool action, int contractId, int salesRepId)
        {
            // CONTRACTTODO 
            //var parameters = new List<SqlParameter>();
            //parameters.Add(new SqlParameter("Debug", 0));
            //parameters.Add(new SqlParameter("SalesRepID", salesRepId));
            //parameters.Add(new SqlParameter("ContractID", contractId));
            //parameters.Add(new SqlParameter("isAutoPay", action));

            //_data.ExecuteNonQuery(AdminConstants.UpdateContract_AutoPay, parameters);
        }

        public void AcceptEChecks(bool action, int contractId)
        {
            // CONTRACTTODO 
            //var parameters = new List<SqlParameter>();
            //parameters.Add(new SqlParameter("acceptEChecks", action));
            //parameters.Add(new SqlParameter("ContractID", contractId));

            //_data.ExecuteNonQuery(AdminConstants.UpdateContract_AcceptEChecks, parameters);
        }

        public void SignContract(bool action, int contractId)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("isSigned", action));
            parameters.Add(new SqlParameter("ContractID", contractId));

            // CONTRACTTODO
            //_data.ExecuteNonQuery(AdminConstants.UpdateContract_Signed, parameters);
        }



        public void ApplyPromo(string invoiceId, string promoId)
        {
            // CONTRACTTODO
            //var parameters = new List<SqlParameter>();
            //parameters.Add(new SqlParameter("InvoiceID", invoiceId));
            //parameters.Add(new SqlParameter("PromoID", promoId));

            //_data.ExecuteNonQuery(AdminConstants.UpdateInvoice_ApplyPromo, parameters);
        }



        public void FrameContract(bool action, int contractId, string loginId)
        {
            if (!action)
            {
                var item = GetContract_AddOn_Result(2, contractId, 0, loginId);
                if (item.ContractId != null) DeleteContract((int)item.ContractId);
            }
            else
            {
                SaveContract(loginId, contractId, 2, 462, DateTime.Now);
            }
        }

        public void SaveContract(string loginId, int contractId, int type, int priceLevelId, DateTime effectiveDate)
        {
            var parameters = new List<SqlParameter> { new SqlParameter("LoginID", loginId) };
            if (contractId > 0)
            {
                parameters.Add(new SqlParameter("ParentContractID", contractId));
            }

            parameters.Add(new SqlParameter("ContractPriceLevelID", priceLevelId));
            parameters.Add(new SqlParameter("EffectiveDate", effectiveDate));
            DBAccessIntResult intResult = _data.ExecuteWithIntReturn(AdminConstants.InsertContract, parameters);

            // insert stripe record, default autopay to false
            parameters = new List<SqlParameter>
            {
                new SqlParameter("@loginId", loginId),
                new SqlParameter("@autoPay", false)
            };
            _data.ExecuteNonQuery(AdminConstants.InsertStripeAuto, parameters);

            if (intResult.Success)
            {
                // Calculate tax for the contract
                ProEstimatorData.DataModel.Contact contact = ProEstimatorData.DataModel.Contact.GetContactForLogins(InputHelper.GetInteger(loginId));
                ProEstimatorData.DataModel.Address address = ProEstimatorData.DataModel.Address.GetForContact(contact.ContactID);

                // TaxManager.CaculateTaxForContract(address, intResult.Value, InputHelper.GetInteger(loginId));
            }
        }

        public void DeleteContract(int contractId)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractID", contractId));
            _data.ExecuteNonQuery(AdminConstants.Delete_Contract, parameters);
        }

        private VmAddOnResult GetContract_AddOn_Result(int type, int contractId, int value, string loginId)
        {
            var parameters = new List<SqlParameter>();
            var dataSet = new DataSet();
            parameters.Add(new SqlParameter("LoginID", loginId));
            parameters.Add(new SqlParameter("ContractTypeID", type));
            parameters.Add(contractId > 0
                ? new SqlParameter("ParentContractID", contractId)
                : new SqlParameter("ContractID", value));

            var item = _data.ExecuteWithTable(AdminConstants.GetContract_AddOn, parameters);
            if (item != null && item.DataTable != null)
            {
                item.DataTable.TableName = AdminConstants.NO_OF_LOGINS_TABLE;
                dataSet.Tables.Add(item.DataTable);
            }

            return (from DataRow row in dataSet.Tables[0].Rows select row.ToModel<VmAddOnResult>()).FirstOrDefault();
        }

        public void EmsContract(bool action, int contractId, string loginId)
        {
            if (!action)
            {
                var item = GetContract_AddOn_Result(5, contractId, 0, loginId);
                if (item.ContractId != null) DeleteContract((int)item.ContractId);
            }
            else
            {
                SaveContract(loginId, contractId, 5, 463, DateTime.Now);
            }
        }

        public void PdrContract(bool action, int contractId, string loginId)
        {
            if (!action)
            {
                var item = GetContract_AddOn_Result(7, contractId, 0, loginId);
                if (item.ContractId != null) DeleteContract((int)item.ContractId);
            }
            else
            {
                SaveContract(loginId, contractId, 7, 482, DateTime.Now);
            }
        }

        public List<GetFrameDataLevelTermsLookup> GetFrameDataLevelTerms(int contractTypeID, bool priceLevelActive,
            bool includeFreeTerms)
        {
            var result = new List<GetFrameDataLevelTermsLookup>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractTypeID", contractTypeID));
            parameters.Add(new SqlParameter("PriceLevelActive", priceLevelActive));
            parameters.Add(new SqlParameter("IncludeFreeTerms", includeFreeTerms));

            var item = _data.ExecuteWithTable(AdminConstants.GET_LEVEL_TERMS, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result =
                    (from DataRow row in ds.Tables[0].Rows select row.ToModel<GetFrameDataLevelTermsLookup>()).ToList();
            }

            return result;
        }

        public List<VmSalesRep> GetSalesRepresentatives()
        {
            var result = new List<VmSalesRep>();
            var parameters = new List<SqlParameter>();

            var item = _data.ExecuteWithTable(AdminConstants.GetSalesReps, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmSalesRep>()).ToList();
            }

            return result;
        }

        public List<VmProfile> GetProfileData()
        {
            var result = new List<VmProfile>();
            var parameters = new List<SqlParameter>();

            var item = _data.ExecuteWithTable(AdminConstants.GetDefaultProfile, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmProfile>()).ToList();
            }

            return result;
        }

        public void DeleteCustomInvoice(int id)
        {
            // TaxManager.VoidInvoice(Invoice.Get(id));

            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@InvoiceID", id));
            _data.ExecuteNonQuery(AdminConstants.DeleteInvoice, parameters);
        }

        public void CreateCustomInvoices(int loginId, int numberOfPayments, decimal invoiceAmount,
            string customPaymentDate, string notes)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@LoginID", loginId));
            parameters.Add(new SqlParameter("@EffectiveDate", customPaymentDate));
            parameters.Add(new SqlParameter("@NumberOfPayments", numberOfPayments));
            parameters.Add(new SqlParameter("@InvoiceAmount", invoiceAmount));
            parameters.Add(new SqlParameter("@Notes", notes));
            _data.ExecuteNonQuery(AdminConstants.InsertInvoice_Custom, parameters);
        }

        public VmSalesRep UpdateSalesRep(SalesRep rep)
        {
            var result = new VmSalesRep();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@FirstName", rep.FirstName));
            parameters.Add(new SqlParameter("@LastName", rep.LastName));
            parameters.Add(new SqlParameter("@Email", rep.Email));
            parameters.Add(new SqlParameter("@UserName", rep.UserName));
            parameters.Add(new SqlParameter("@Password", rep.Password));
            parameters.Add(new SqlParameter("@SalesRepID", rep.SalesRepID));

            var item = _data.ExecuteWithTable(AdminConstants.UpdateSalesRepSql, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmSalesRep>()).FirstOrDefault();
            }

            ProEstimatorData.DataModel.SalesRep.RefreshCache();

            return result;
        }

        public void UpdateSalesRepPermission(VmSalesRep rep)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ID", rep.ID));
            parameters.Add(new SqlParameter("pInvoiceTab", rep.InvoiceTab));
            parameters.Add(new SqlParameter("pOrgMaintTab", rep.OrgMaintTab));
            parameters.Add(new SqlParameter("pSalesRepMaint", rep.SalesRepMaint));
            parameters.Add(new SqlParameter("pSalesBoard", rep.SalesBoard));
            parameters.Add(new SqlParameter("pServerLogsTab", rep.ServerLogsTab));
            parameters.Add(new SqlParameter("pLoginFailureTab", rep.LoginFailureTab));
            parameters.Add(new SqlParameter("pErrorsTab", rep.ErrorsTab));
            parameters.Add(new SqlParameter("pCurrentSessionsTab", rep.CurrentSessionsTab));
            parameters.Add(new SqlParameter("pLinkingTab", rep.LinkingTab));
            parameters.Add(new SqlParameter("pPaymentReport", rep.PaymentReport));
            parameters.Add(new SqlParameter("pForcastedRevReport", rep.ForcastedRevReport));
            parameters.Add(new SqlParameter("pRenewalReport", rep.RenewalReport));
            parameters.Add(new SqlParameter("pRoyaltyReport", rep.RoyaltyReport));
            parameters.Add(new SqlParameter("pExpectedRenReport", rep.ExpectedRenReport));
            parameters.Add(new SqlParameter("pShopActivityReport", rep.ShopActivityReport));
            parameters.Add(new SqlParameter("pWebsiteAccessReport", rep.WebsiteAccessReport));
            parameters.Add(new SqlParameter("pUnusedContracts", rep.UnusedContracts));
            parameters.Add(new SqlParameter("pEstimatesByShop", rep.EstimatesByShop));
            parameters.Add(new SqlParameter("pUserMaintLoginInfo", rep.UserMaintLoginInfo));
            parameters.Add(new SqlParameter("pUserMaintOrgInfo", rep.UserMaintOrgInfo));
            parameters.Add(new SqlParameter("pUserMaintContactInfo", rep.UserMaintContactInfo));
            parameters.Add(new SqlParameter("pUserMaintSalesRep", rep.UserMaintSalesRep));
            parameters.Add(new SqlParameter("pEditPermissions", rep.EditPermissions));
            parameters.Add(new SqlParameter("pEditBonusGoals", rep.EditBonusGoals));
            parameters.Add(new SqlParameter("pPromoMaintenance", rep.PromoMaintenance));
            parameters.Add(new SqlParameter("pExtensionReport", rep.ExtensionReport));
            parameters.Add(new SqlParameter("pUserMaintImpersonate", rep.UserMaintImpersonate));
            parameters.Add(new SqlParameter("pLoginAttempts", rep.LoginAttempts));
            parameters.Add(new SqlParameter("pUserMaintCreate", rep.UserMaintCreate));
            parameters.Add(new SqlParameter("pImport", rep.Import));
            parameters.Add(new SqlParameter("pImportEst", rep.ImportEst));
            parameters.Add(new SqlParameter("pDeleteRenewal", rep.RenewalReportDelete));

            parameters.Add(new SqlParameter("pCarfax", rep.CarFax));
            parameters.Add(new SqlParameter("pSuccessBox", rep.SuccessBox));

            parameters.Add(new SqlParameter("pPartsNowManager", rep.PartsNowManager));
            parameters.Add(new SqlParameter("pAddOns", rep.AddOns));

            parameters.Add(new SqlParameter("pDocumentsManagerUpload", rep.DocumentsManagerUpload));
            parameters.Add(new SqlParameter("pDocumentsManagerDownload", rep.DocumentsManagerDownload));

            parameters.Add(new SqlParameter("pTrialSetup", rep.TrialSetup));

            _data.ExecuteNonQuery(AdminConstants.UpdateSalesRepPermissionSql, parameters);

            ProEstimatorData.DataModel.SalesRep.RefreshCache();
        }

        public Dictionary<string, Dictionary<string, string>> SaveSalesReps(string action,
            Dictionary<string, Dictionary<string, string>> data)
        {
            var rep = new VmSalesRep().MapFromDataTableRow(data);

            SalesRep salesRep = new SalesRep();
            salesRep.SalesNumber = rep.Number;
            salesRep.FirstName = rep.FirstName;
            salesRep.LastName = rep.LastName;
            salesRep.Email = rep.Email;
            salesRep.UserName = rep.UserName;
            salesRep.Password = rep.Password;
            salesRep.SalesRepID = rep.ID;

            rep = UpdateSalesRep(salesRep);
            return rep.MapToDataTableRow();
        }

        public void InsertSalesRep(SalesRep model)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@FirstName", model.FirstName));
            parameters.Add(new SqlParameter("@LastName", model.LastName));
            parameters.Add(new SqlParameter("@Email", model.Email));
            parameters.Add(new SqlParameter("@SalesNumber", model.SalesNumber));
            _data.ExecuteNonQuery(AdminConstants.InsertSalesRepSql, parameters);

            ProEstimatorData.DataModel.SalesRep.RefreshCache();
        }

        public void DeleteRep(int id)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@SalesRepID", id));
            _data.ExecuteNonQuery(AdminConstants.DeleteSalesRep, parameters);

            ProEstimatorData.DataModel.SalesRep.RefreshCache();
        }

        public VmSalesRep GetSalesRep(int id)
        {
            var result = new VmSalesRep();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@SalesRepID", id));

            var item = _data.ExecuteWithTable(AdminConstants.GetSalesReps, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmSalesRep>()).FirstOrDefault();
            }

            return result;
        }

        public List<VmExtensionHistory> GetExtensionHistory(int loginId)
        {
            var result = new List<VmExtensionHistory>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@id", loginId));

            var item = _data.ExecuteWithTable(AdminConstants.GetExtensionHistorySql, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmExtensionHistory>()).ToList();
            }

            return result;
        }

        public void UpdateEstimatingDates(string effectiveDate, string expirationDate, int contractId)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@effectiveDate", effectiveDate.FormatDate()));
            parameters.Add(new SqlParameter("@expirationDate", expirationDate.FormatDate()));
            parameters.Add(new SqlParameter("@contractId", contractId));
            // CONTRACTTODO
            //_data.ExecuteNonQuery(AdminConstants.UpdateEstimatingDatesSql, parameters);
        }

        public void ShiftDueDates(string date, string contractId, string loginId)
        {
            // CONTRACTTODO
            //var contract = GetInvoice(loginId, contractId);
            //var invoice = contract.Details.FirstOrDefault();
            //if (invoice != null)
            //{
            //    DateTime oDate;
            //    DateTime myDate;
            //    if (DateTime.TryParse(invoice.DueDate, out oDate) && DateTime.TryParse(date, out myDate))
            //    {
            //        var shift = (myDate - oDate).TotalDays;
            //        foreach (var item in contract.Details)
            //        {
            //            var shiftedDate =
            //                FormatExpireDate(
            //                    DateTime.Parse(item.DueDate).AddDays(shift).ToString(CultureInfo.InvariantCulture));
            //            var parameters = new List<SqlParameter>();
            //            parameters.Add(new SqlParameter("@dueDate", shiftedDate));
            //            parameters.Add(new SqlParameter("@id", item.Id));
            //            _data.ExecuteNonQuery(AdminConstants.UpdateInvoiceSql, parameters);
            //        }
            //    }
            //}
        }

        public Dictionary<string, Dictionary<string, string>> SaveInvoices(string action,
            Dictionary<string, Dictionary<string, string>> data)
        {
            var invoice = new VmInvoiceRow().MapFromDataTableRow(data);
            invoice = UpdateInvoice(invoice);
            return invoice.MapToDataTableRow();
        }

        private VmInvoiceRow UpdateInvoice(VmInvoiceRow invoice)
        {
            //CONTRACTTODO
            var result = new VmInvoiceRow();
            //var parameters = new List<SqlParameter>();
            //parameters.Add(new SqlParameter("@id", invoice.Id));
            //parameters.Add(new SqlParameter("@Amount", invoice.Amount));
            //parameters.Add(new SqlParameter("@SalesTax", invoice.SalesTax));
            //parameters.Add(new SqlParameter("@DueDate", invoice.DueDate));

            //var item = _data.ExecuteWithTable(AdminConstants.UpdateSelectInvoice, parameters);
            //if (item != null && item.DataTable != null)
            //{
            //    var ds = new DataSet();
            //    ds.Tables.Add(item.DataTable);
            //    var prelim = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmInvoiceRow>()).ToList();
            //    result = prelim.FirstOrDefault(row => row.Id == invoice.Id);
            //}

            return result;
        }

        public VmImport GetImporter()
        {
            var result = new VmImport();

            return result;
        }

        private void FillEstimateIDList(int loginID)
        {
            var dbAccess = new DBAccess();
            var tableResult = dbAccess.ExecuteWithTable("DataMigration_GetUnimportedEstimates",
                new SqlParameter("LoginID", loginID));
            if (tableResult.Success)
            {
                var estimateIDS = new List<int>();
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    var estimateID = InputHelper.GetInteger(row["id"].ToString());
                    if (!estimateIDS.Contains(estimateID))
                    {
                        estimateIDS.Add(estimateID);
                    }
                }
                var est = EstimateCollection.GetInstance;
                est.RemoveForLogin(loginID);
                est.AddEstimateRange(new LoginEstimate { Key = loginID, IDs = estimateIDS });
            }
        }

        public VmImport GetLoginInfo(string loginId, string message)
        {
            var result = new VmImport { Message = !string.IsNullOrEmpty(message) ? message : "" };
            int myloginId;
            if (!string.IsNullOrEmpty(loginId) &&
                int.TryParse(loginId, out myloginId))
            {
                var item = _data.ExecuteWithTable("DataMigration_GetLoginInfo", new SqlParameter("LoginID", loginId));
                if (item.Success)
                {
                    if (item.DataTable != null && item.DataTable.Rows.Count > 0)
                    {
                        FillEstimateIDList(myloginId);

                        result.LoginID = myloginId;
                        result.LoginName = item.DataTable.Rows[0]["Organization"].ToString();
                        result.LoginImported =
                            InputHelper.GetBoolean(item.DataTable.Rows[0]["LoginIsImported"].ToString());
                        result.SourceEstimates =
                            InputHelper.GetInteger(item.DataTable.Rows[0]["EstimatesOld"].ToString());
                        result.UnimportedEstimates = result.SourceEstimates -
                                                     InputHelper.GetInteger(
                                                         item.DataTable.Rows[0]["EstimatesNew"].ToString());

                        if (string.IsNullOrEmpty(result.LoginName))
                        {
                            result.LoginName = "--Not Found--";
                        }
                    }
                }
                else
                {
                    result.Message = item.ErrorMessage;
                }
                result.LoginID = Convert.ToInt32(loginId);
            }
            else
            {
                result.Message = "Enter a valid loginID.";
            }

            //#if DEBUG
            //            result.LoginID = int.Parse(loginId);
            //            result.LoginName = "Test Organization";
            //            result.LoginImported = true;
            //            result.SourceEstimates = 100;
            //            result.UnimportedEstimates = 0;
            //            result.Message = string.Empty;
            //#endif

            return result;
        }

        public VmImport ImportLogin(string loginIDString, int salesRepID = 0, string trialEndDateStr = null)
        {
            int loginID = InputHelper.GetInteger(loginIDString);
            if (loginID == 0)
            {
                ErrorLogger.LogError("Invalid parameter loginId: int value expected", 0, 0, "Login Import");
                return new VmImport();
            }

            var queryResult = _data.ExecuteNonQuery("DataMigration_Logins", new SqlParameter("LoginID", loginID), 0);
            if (queryResult.Success)
            {
                // Log the successfull import
                List<SqlParameter> logParams = new List<SqlParameter>();
                logParams.Add(new SqlParameter("LoginID", loginID));

                if (salesRepID > 0)
                {
                    logParams.Add(new SqlParameter("SalesRepID", salesRepID));
                }
                else
                {
                    logParams.Add(new SqlParameter("SelfImport", true));
                }
                _data.ExecuteNonQuery("LoginImportDataInsert", logParams);

                ImportCompanyLogo(loginID);

                if(trialEndDateStr == null)
                {
                    CreateTrialForWEImport(loginID);
                }
                else
                {
                    DateTime startDate = DateTime.Now.Date;
                    DateTime endDate = InputHelper.GetDateTime(trialEndDateStr);

                    ContractManager.CreateTrial(loginID, startDate, endDate, false, false, false, false, true, true, false,false);
                }

                // Create the first user
                try
                {
                    LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                    Contact mainContact = Contact.GetContact(loginInfo.ContactID);
                    FunctionResultInt newUserResult = ProEstimator.Business.Logic.LoginManager<SiteActiveLogin>.CreateUser(loginID, mainContact.Email, loginID.ToString(), true);
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, loginID, "Import first user");
                }

                return GetLoginInfo(loginID.ToString(), "The login has been successfully imported");
            }

            ErrorLogger.LogError(queryResult.ErrorMessage, loginID, 0, "Login Import");
            return GetLoginInfo(loginID.ToString(), "Error importing login: " + queryResult.ErrorMessage);
        }

        private void ImportCompanyLogo(int loginID)
        {
            try
            {
                DBAccess db = new DBAccess();
                DBAccessStringResult stringResult = db.ExecuteWithStringReturn("DataMigration_GetCompanyLogo", new SqlParameter("LoginID", loginID));
                if (stringResult.Success)
                {
                    if (File.Exists(stringResult.Value))
                    {
                        string extension = Path.GetExtension(stringResult.Value).Replace(".", "");
                        string targetPath = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["BaseURL"], "UserContent", "CompanyLogos", loginID.ToString() + "." + extension);

                        File.Copy(stringResult.Value, targetPath);

                        LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                        loginInfo.LogoImageType = extension;
                        loginInfo.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "ImportCompanyLogo");
            }
        }

        private void CreateTrialForWEImport(int loginID)
        {
            // Make a Trial contract
            DBAccess db = new DBAccess();
            DBAccessStringResult stringResult = db.ExecuteWithStringReturn("DataMigration_GetWEExpirationDate", new SqlParameter("LoginID", loginID));

            DateTime trialStartDate = DateTime.Now.Date;
            if (stringResult.Success)
            {
                DateTime weExpiration = InputHelper.GetDateTime(stringResult.Value);
                if (weExpiration > DateTime.Now && weExpiration < DateTime.Now.AddDays(14))
                {
                    trialStartDate = weExpiration.AddDays(-14).Date;
                }
            }

            ContractManager.CreateTrial(loginID, trialStartDate, trialStartDate.AddDays(14), false, false, false, false, true, true, false,false);
        }

        public VmImport DeleteLogin(string loginId)
        {
            var returnMessage = "";

            var queryResult = _data.ExecuteNonQuery("aaaDeleteLoginsData", new SqlParameter("LoginsID", loginId), 0);
            if (queryResult.Success)
            {
                returnMessage = "Loging data deleted.";
            }
            else
            {
                returnMessage = "Error deleting login: " + queryResult.ErrorMessage;
            }

            queryResult = _data.ExecuteNonQuery("aaaaDeleteEstimates", new SqlParameter("LoginID", loginId), 0);
            if (queryResult.Success)
            {
                returnMessage += Environment.NewLine + "Estimates deleted.";
            }
            else
            {
                returnMessage += Environment.NewLine + "Error deleting estimates: " + queryResult.ErrorMessage;
            }

            return GetLoginInfo(loginId, returnMessage);
        }

        public VmImportEstimate ImportContracts(string loginId, string estimateId)
        {
            var result = new VmImportEstimate();

            var dbAccess = new DBAccess();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var contractMigrationResult = dbAccess.ExecuteNonQuery(AdminConstants.DataMigration_Contracts, new SqlParameter("LoginID", loginId), 0);
            result.TimingMessage = "Contract imported in " + stopwatch.Elapsed;
            if (contractMigrationResult.Success)
            {
                result.Success = true;
                result.Message = "Done importing data!";
            }
            else
            {
                result.ErrorMessage = contractMigrationResult.ErrorMessage;
            }

            return result;
        }

        public VmImportEstimate ImportEstimate(string loginId, string estimateId)
        {
            var result = new VmImportEstimate();

            var estimateIdInt = 0;
            var loginIdInt = 0;

            if (!string.IsNullOrEmpty(estimateId))
            {
                estimateIdInt = InputHelper.GetInteger(estimateId);

                if (estimateIdInt == 0)
                {
                    result.ErrorMessage = "Invalid estimate ID number.";
                }
            }
            else
            {
                if (int.TryParse(loginId, out loginIdInt) &&
                    EstimateCollection.GetInstance.GetEstimatesForLogin(loginIdInt) != null &&
                    EstimateCollection.GetInstance.GetEstimatesForLogin(loginIdInt).IDs.Count > 0)
                {
                    estimateIdInt = EstimateCollection.GetInstance.GetEstimatesForLogin(loginIdInt).IDs[0];
                }
                else
                {
                    result.ErrorMessage = "No more estimate IDs to import";
                }

                //if(string.IsNullOrEmpty(loginId))
                //{
                //    loginId = "11111";
                //}
                //ImportEstimateViaQueue(loginId, estimateId);
            }

            if (estimateIdInt > 0)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var dbAccess = new DBAccess();
                var queryResult = dbAccess.ExecuteNonQuery(AdminConstants.DataMigration_Estimate, new SqlParameter("AdminInfoID", estimateIdInt), 0);

                var est = EstimateCollection.GetInstance;
                var col = est.GetEstimates(estimateIdInt);
                col.IDs.Remove(estimateIdInt);

                //if (col.IDs.Count == 0)
                //{
                //    dbAccess.ExecuteNonQuery(AdminConstants.Update_ConversionComplete, new List<SqlParameter>() { new SqlParameter("loginId", loginId), new SqlParameter("conversionComplete", true) }, 0);
                //    result.Message = "Conversion complete!";
                //}

                // Return HasNextEstimate only if no estimate ID was passed.  If HasNextEstimate returns true, the next estimate will be imported, but we don't want that
                // to happen if a specific estimate was passed by the user.
                if (string.IsNullOrEmpty(estimateId) && loginIdInt > 0)
                {
                    result.HasNextEstimate = est.GetEstimatesForLogin(loginIdInt) != null &&
                                             est.GetEstimatesForLogin(loginIdInt).IDs.Count > 0;
                }
                else
                {
                    result.HasNextEstimate = false;
                }

                result.RemainingEstimateCount = col.IDs.Count;
                result.TimingMessage = "Estimate imported in " + stopwatch.Elapsed;
                if (queryResult.Success)
                {
                    result.Success = true;
                    result.Message = "Done importing data!";
                }
                else
                {
                    result.ErrorMessage = queryResult.ErrorMessage;
                }
            }

            return result;
        }

        public VmImportEstimate ImportEstimateViaQueue(string loginId, string estimateId)
        {
            var result = new VmImportEstimate();
            if (!string.IsNullOrEmpty(estimateId))
            {
                ImportEstimate(loginId, estimateId);
            }
            else
            {
                //place estimate on the queue

                using (var client = new PEService.ImportServiceClient())
                {
                    var request = new PEService.QueueImportRequest();
                    int myLogin;
                    if (int.TryParse(loginId, out myLogin))
                    {
                        request.LoginId = myLogin;
                        request.Status = PEService.ImportStatus.Waiting;
                        request.Type = PEService.ImportType.Contracts;
                        request.Source = PEService.ImportSource.Admin;
                        client.AddImportToQueueAsync(request);
                        result.Success = true;
                        result.LoginId = myLogin;
                    }
                }
            }

            return result;
        }

        public VmPaymentReport GetPaymentReportDetail(VmPaymentReport report)
        {
            var result = new VmPaymentReport();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@startDate", report.FromDate));
            parameters.Add(new SqlParameter("@endDate", report.ToDate));

            var sp = _data.ExecuteWithTable(AdminConstants.GetStripePayments, parameters);
            if (sp != null && sp.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(sp.DataTable);
                result.StripePayments = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmStripePayment>()).ToList();
            }

            return result;
        }

        public void ToggleAutoPay(int loginId, bool action, int contractId)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@loginId", loginId));
            parameters.Add(new SqlParameter("@autoPay", action));
            _data.ExecuteNonQuery(AdminConstants.InsertStripeAuto, parameters);
            parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@loginId", loginId));
            parameters.Add(new SqlParameter("@autoPay", action));
            _data.ExecuteNonQuery(AdminConstants.UpdateStripeAuto, parameters);
        }

        public List<VmSmsHistory> GetHistory()
        {
            return GetHistoryRecords();
        }

        public List<VmSmsHistory> GetHistoryByDate(DateTime fromDate, DateTime toDate)
        {
            return GetHistoryRecords().Where(x => DateTime.Parse(x.DateSent) >= fromDate && DateTime.Parse(x.DateSent) <= toDate.AddDays(1)).ToList();
        }

        public List<VmPdrReport> GetPdrByDate(DateTime fromDate, DateTime toDate)
        {
            return GetPdrRecords().Where(x => DateTime.Parse(x.Date) >= fromDate && DateTime.Parse(x.Date) <= toDate.AddDays(1)).ToList();
        }

        private List<VmSmsHistory> GetHistoryRecords()
        {
            var result = new List<VmSmsHistory>();
            var parameters = new List<SqlParameter>();

            var item = _data.ExecuteWithTable(AdminConstants.GET_SmsHistory, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result =
                    (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmSmsHistory>()).OrderByDescending(x => x.DateSent)
                        .ToList();
            }

            return result;
        }

        private List<VmPdrReport> GetPdrRecords()
        {
            var result = new List<VmPdrReport>();
            var parameters = new List<SqlParameter>();

            var item = _data.ExecuteWithTable(AdminConstants.GET_PdrReport, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result =
                    (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmPdrReport>()).OrderByDescending(x => x.Date)
                        .ToList();
            }

            return result;
        }

        public async Task<bool> SendAdminSms(VmSmsHistory item)
        {
            var result = false;
            if (item.SalesRepId != null && item.LoginId != null)
            {
                //for (int i = 0; i <= 100; i++)
                //{
                //    Thread.Sleep(11000);
                //    response = await _signUpService.SendNewCustomerSMSCustomMessage((int)item.SalesRepId, (int)item.LoginId, item.PhoneNumber, string.Format("{0} counter: {1}", item.Message, i)).ConfigureAwait(false);
                //    if (!response.Success)
                //        break;
                //}
                var response = await _signUpService.SendNewCustomerSMSCustomMessage((int)item.SalesRepId, (int)item.LoginId, item.PhoneNumber, string.Format("{0}", item.Message)).ConfigureAwait(false);

                AddHistory(item);

                if (response != null) result = response.Success;
            }

            return result;
        }

        public void AddHistory(VmSmsHistory item)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@PhoneNumber", item.PhoneNumber));
            parameters.Add(new SqlParameter("@Message", item.Message));
            parameters.Add(new SqlParameter("@SalesRepId", item.SalesRepId));
            _data.ExecuteNonQuery(AdminConstants.Insert_SmsHistory, parameters);
        }

        public List<VmRenewalRecord> GetRenewalByDate(int month, int year)
        {
            //return GetRenewalRecords().Where(x => DateTime.Parse(x.RenewalDate) >= fromDate && DateTime.Parse(x.RenewalDate) <= toDate.AddDays(1)).ToList();
            var renewals = GetRenewalRecords(month, year);
            renewals.RemoveAll(renewal => renewal.Platform == "WE" && renewals.Any(PE => PE.LoginID == renewal.LoginID && PE.Platform == "PE"));

            return renewals;
        }

        public Dictionary<string, Dictionary<string, string>> SaveRenewal(string action, Dictionary<string, Dictionary<string, string>> data)
        {
            Dictionary<string, Dictionary<string, string>> result = null;
            if (action == "remove")
            {
                var renewal = new VmRenewalRecord().MapFromDataTableRow(data);
                DeleteRenewal(renewal.Id);
                result = new Dictionary<string, Dictionary<string, string>>();
            }
            else
            {
                var renewal = new VmRenewalRecord().MapFromDataTableRow(data);
                renewal = UpdateRenewal(renewal);
                result = renewal.MapToDataTableRow();
            }

            return result;
        }

        private void DeleteRenewal(int id)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@id", id));

            _data.ExecuteNonQuery(AdminConstants.DeleteRenewal, parameters);
        }

        private VmRenewalRecord UpdateRenewal(VmRenewalRecord renewal)
        {
            var result = new VmRenewalRecord();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@id", renewal.Id));
            parameters.Add(new SqlParameter("@WillRenew", renewal.WillRenew));
            parameters.Add(new SqlParameter("@WillNotRenew", renewal.WillNotRenew));
            parameters.Add(new SqlParameter("@Notes", renewal.Notes));

            var item = _data.ExecuteWithTable(AdminConstants.UpdateSelectRenewal, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                var prelim = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmRenewalRecord>()).ToList();
                result = prelim.FirstOrDefault(row => row.Id == renewal.Id);
            }

            return result;
        }

        public VmTrialReport GetTrialsByDate(DateTime fromDate, DateTime toDate, int repId)
        {
            //return GetRenewalRecords().Where(x => DateTime.Parse(x.RenewalDate) >= fromDate && DateTime.Parse(x.RenewalDate) <= toDate.AddDays(1)).ToList();

            var result = new VmTrialReport();
            result.Records = GetTrialRecords(fromDate, toDate);
            result.Records.ForEach(record =>
            {
                //if (!record.Trial)
                //{
                //    record.HasConverted = true;
                //}
                //else
                //{
                //    record.HasConverted = result.Records.Exists(x => x.Id == record.Id && !x.Trial);
                //}
                Contract activeContract = Contract.GetActive(record.Id);
                record.HasConverted = activeContract != null;
            });

            result.Records = result.Records.OrderBy(x => x.TrialStartDate).ToList();
            //return result.Where(r => r.Trial).ToList();
            result.GraphData = result.ToGraphObject(result.Records);
            result.SingleRepGraphData = result.ToFilteredGraphObject(result.Records, repId);

            result.TotalWeTrialDetail = result.Records.Where(x => x.Trial).ToList();
            result.TotalActiveWeTrialDetail = result.Records.Where(x => x.Trial && DateTime.Parse(x.TrialStartDate) <= DateTime.Now && DateTime.Parse(x.TrialEndDate) >= DateTime.Now).ToList();
            result.TotalWeTrialsConvertedDetail = result.Records
                .Where(x => x.HasConverted && !x.Trial)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();

            result.TotalWeTrials = result.TotalWeTrialDetail.Count();
            result.TotalActiveWeTrials = result.TotalActiveWeTrialDetail.Count();
            result.TotalWeTrialsConverted = result.TotalWeTrialsConvertedDetail.Count();

            return result;
        }

        private List<VmRenewalRecord> GetRenewalRecords(int month, int year)
        {
            var result = new List<VmRenewalRecord>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@month", month));
            parameters.Add(new SqlParameter("@year", year));

            var item = _data.ExecuteWithTable(AdminConstants.GET_RenewalReport, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result =
                    (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmRenewalRecord>()).OrderByDescending(x => x.RenewalDate)
                        .ToList();
            }

            return result;
        }

        private List<VmTrialRecord> GetTrialRecords(DateTime fromDate, DateTime toDate)
        {
            var result = new List<VmTrialRecord>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@StartDate", fromDate));
            parameters.Add(new SqlParameter("@EndDate", toDate.AddDays(1)));

            var item = _data.ExecuteWithTable(AdminConstants.GET_TRIAL_REPORT, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result =
                    (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmTrialRecord>()).OrderByDescending(x => x.TrialStartDate)
                        .ToList();
            }

            return result;
        }

        public List<ImageInfo> GetImagesForLogin(int login)
        {
            var result = new List<ImageInfo>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@LoginId", login));
            var item = _data.ExecuteWithTable(AdminConstants.GET_MigrateImages, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result =
                    (from DataRow row in ds.Tables[0].Rows select row.ToModel<ImageInfo>())
                        .ToList();
            }

            return result;
        }

        private static void CopyImages(List<ImageInfo> info)
        {
            foreach (var item in info)
            {
                var PePath = string.Format(@"D:\ProEstimatorStorage\{0}\{1}\{2}", item.LoginId, item.EstimateId, "Images");
                System.Console.WriteLine(string.Format("Creating Directory: {0}", PePath));
                Directory.CreateDirectory(PePath);
                System.Console.WriteLine(string.Format("Copying from: {0} , Copying to: {1}", item.WePath, string.Format(@"{0}\{1}", PePath, item.FileName)));
                if (!File.Exists(string.Format(@"{0}\{1}", PePath, item.FileName)))
                {
                    File.Copy(string.Format(@"{0}", item.WePath), string.Format(@"{0}\{1}", PePath, item.FileName));
                }
                if (!File.Exists(string.Format(@"{0}\{1}", PePath, item.ThumbFileName)))
                {
                    File.Copy(string.Format(@"{0}", item.WePath), string.Format(@"{0}\{1}", PePath, item.ThumbFileName));
                }
            }
        }

        public void MigrateImages(int loginId)
        {
            var info = GetImagesForLogin(loginId);
            CopyImages(info);
        }

        internal void LogException(int loginId, string errorMessage)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@LoginId", loginId));
            parameters.Add(new SqlParameter("@Exception", errorMessage));
            _data.ExecuteNonQuery(AdminConstants.LOG_EXCEPTION, parameters);
        }

        internal VmRatio GetEstimateCountsById(int loginId)
        {
            var result = new VmRatio();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@LoginId", loginId));
            var item = _data.ExecuteWithTable(AdminConstants.DataMigration_GetEstimateMigrationMetricForLogin, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmRatio>()).FirstOrDefault();
            }

            return result;
        }

        public void RunRenewalReport(string platform)
        {
            //run report for 12 month interval
            for (int i = 0; i <= 11; i++)
            {
                var parameters = new List<SqlParameter>();
                var now = DateTime.Now;
                var month = now.AddMonths(i).Month;
                var year = now.AddMonths(i).Year;
                //Console.WriteLine("month: {0}, year {1}", month, year);
                parameters.Add(new SqlParameter("@month", month));
                parameters.Add(new SqlParameter("@year", year));

                if (platform == "PE")
                {
                    _data.ExecuteNonQuery(AdminConstants.UpdateRenewalReport_PE, parameters);
                }
                if (platform == "WE")
                {
                    _data.ExecuteNonQuery(AdminConstants.UpdateRenewalReport_WE, parameters);
                }
            }
        }
    }
}