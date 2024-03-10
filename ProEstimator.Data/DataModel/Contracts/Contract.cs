using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class Contract : ProEstEntity
    {
        public int ID { get; private set; }
        public int LoginID { get; set; }
        public ContractPriceLevel ContractPriceLevel { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool IsSigned { get; set; }
        public string Notes	{ get; set; }
        public int PromoID { get; set; }
        public bool Active{ get; set; }
        public DateTime DateCreated	{ get; set; }
        public bool WillRenew { get; set; }
        public bool WillNotRenew { get; set; }
        public bool IsDeleted { get; set; }
        public bool AutoRenew { get; set; }
        public string CompanyName { get; set; }
        public string CompanyContactName { get; set; }
        public string ContractTermDescription { get; set; }
        public bool IgnoreAutoPay { get; set; }
        public bool EarlyRenewal { get; set; }

        /// <summary>
        /// This is not a database field, this is calculated when data is loaded.  If any Invoices attached to this contract are paid, this will be True.
        /// </summary>
        public bool HasPayment { get; private set; }

        public DateTime ExpirationDate { get; set; }
        private DateTime _databaseExpirationDate;

        public int DaysUntilExpiration
        {
            get
            {
                TimeSpan span = ExpirationDate - DateTime.Now;
                return (int)span.TotalDays;
            }
        }

        public Contract()
        {

        }

        public Contract(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ContractID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            ContractPriceLevel = ContractPriceLevel.Get(InputHelper.GetInteger(row["ContractPriceLevelID"].ToString()));
            EffectiveDate = InputHelper.GetDateTime(row["EffectiveDate"].ToString()).Date;
            ExpirationDate = InputHelper.GetDateTime(row["ExpirationDate"].ToString()).Date;
            _databaseExpirationDate = InputHelper.GetDateTime(row["ExpirationDate"].ToString());
            IsSigned = InputHelper.GetBoolean(row["IsSigned"].ToString());
            Notes = row["Notes"].ToString();
            PromoID = InputHelper.GetInteger(row["PromoID"].ToString());
            Active = InputHelper.GetBoolean(row["Active"].ToString());
            DateCreated = InputHelper.GetDateTime(row["DateCreated"].ToString());
            WillRenew = InputHelper.GetBoolean(row["WillRenew"].ToString());
            WillNotRenew = InputHelper.GetBoolean(row["WillNotRenew"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());
            AutoRenew = InputHelper.GetBoolean(row["AutoRenew"].ToString());
            IgnoreAutoPay = InputHelper.GetBoolean(row["IgnoreAutoPay"].ToString());
            EarlyRenewal = InputHelper.GetBoolean(row["EarlyRenewal"].ToString());

            try
            {
                HasPayment = InputHelper.GetBoolean(row["HasPayment"].ToString());
            }
            catch
            {
                HasPayment = false;
            }

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("ContractPriceLevelID", ContractPriceLevel.ID));
            parameters.Add(new SqlParameter("EffectiveDate", EffectiveDate));
            parameters.Add(new SqlParameter("ExpirationDate", ExpirationDate));
            parameters.Add(new SqlParameter("IsSigned", IsSigned));
            parameters.Add(new SqlParameter("Notes", InputHelper.GetString(Notes)));
            parameters.Add(new SqlParameter("PromoID", PromoID));
            parameters.Add(new SqlParameter("Active", Active));
            parameters.Add(new SqlParameter("DateCreated", DateCreated));
            parameters.Add(new SqlParameter("WillRenew", WillRenew));
            parameters.Add(new SqlParameter("WillNotRenew", WillNotRenew));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));
            parameters.Add(new SqlParameter("AutoRenew", AutoRenew));
            parameters.Add(new SqlParameter("IgnoreAutoPay", IgnoreAutoPay));
            parameters.Add(new SqlParameter("EarlyRenewal", EarlyRenewal));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateContract", parameters);

            if (result.Success)
            {
                if (_databaseExpirationDate.Year > 2000 && _databaseExpirationDate != ExpirationDate)
                {
                    int salesRepID = 0;

                    if (activeLoginID > 0)
                    {
                        ActiveLogin activeLogin = ActiveLogin.Get(activeLoginID);
                        salesRepID = activeLogin.SalesRepID;
                    }

                    ContractEndDateChangeLog log = new ContractEndDateChangeLog();
                    log.ContractID = ID;
                    log.OldDate = _databaseExpirationDate.Date;
                    log.NewDate = ExpirationDate.Date;
                    log.SalesRepID = salesRepID;
                    log.Save();

                    _databaseExpirationDate = ExpirationDate;
                }

                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "Contract", ID, LoginID, parameters, RowAsLoaded);
            }
          
            return new SaveResult(result);
        }

        public static SaveResult SaveWEContractWillRenew(int contractID, bool willRenew)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractID", contractID));
            parameters.Add(new SqlParameter("WillRenew", willRenew));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("UpdateWEContractWillRenew", parameters);

            return new SaveResult(result);
        }

        public static Contract Get(int contractID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetContractByID", new SqlParameter("ContractID", contractID));
            if (result.Success)
            {
                return new Contract(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static Contract GetHeaderContractInvoicesReport(int contractID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("[ContractInvoicesReport_GetHeader]", new SqlParameter("ContractID", contractID));
            if (result.Success)
            {
                Contract contract = new Contract();
                DataRow row = result.DataTable.Rows[0];
                contract.CompanyName = InputHelper.GetString(row["CompanyName"].ToString());
                contract.CompanyContactName = InputHelper.GetString(row["CompanyContactName"].ToString());
                contract.ContractTermDescription = InputHelper.GetString(row["ContractTermDescription"].ToString());
                contract.EffectiveDate = InputHelper.GetDateTime(row["ContractsEffectiveDate"].ToString());
                contract.ExpirationDate = InputHelper.GetDateTime(row["ContractsExpirationDate"].ToString());

                return contract;
            }

            return null;
        }

        public static Contract GetActive(int loginID)
        {
            List<Contract> contracts = GetAllForLogin(loginID);
            return GetActive(contracts);
        }

        public static Contract GetActive(List<Contract> allContracts)
        {
            foreach (Contract contract in allContracts.OrderByDescending(o => o.ExpirationDate))
            {
                if (contract.Active && contract.HasPayment && !contract.IsDeleted && contract.EffectiveDate.Date <= DateTime.Now.Date && contract.ExpirationDate.Date >= DateTime.Now.Date)
                {
                    return contract;
                }
            }

            return null;
        }

        public static Contract GetInProgress(int loginID)
        {
            List<Contract> contracts = GetAllForLogin(loginID);

            foreach (Contract contract in contracts)
            {
                if (contract.Active && !contract.HasPayment && !contract.IsDeleted && contract.ExpirationDate >= DateTime.Now.Date)
                {
                    return contract;
                }
            }

            return null;
        }

        public static List<Contract> GetAllForLogin(int loginID, bool includeDeleted = false)
        {
            List<Contract> list = new List<Contract>();

            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("Deleted", includeDeleted));

            DBAccessTableResult result = db.ExecuteWithTable("GetContractsByLoginID", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new Contract(row));
                }
            }

            return list;
        }

        public static List<Contract> GetContracts(string loginID, string rangeStart, string rangeEnd, string salesRep)
        {
            List<Contract> contracts = new List<Contract>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", InputHelper.GetInteger(loginID)));
            parameters.Add(new SqlParameter("StartDate", InputHelper.GetDateTime(rangeStart)));
            parameters.Add(new SqlParameter("EndDate", InputHelper.GetDateTime(rangeEnd)));
            parameters.Add(new SqlParameter("SalesRepID", InputHelper.GetInteger(salesRep)));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetContractsNoPayment", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    Contract contract = new Contract(row);
                    contract.CompanyContactName = InputHelper.GetString(row["SalesRep"].ToString());
                    contracts.Add(contract);
                }
            }

            return contracts;
        }

    }
}
