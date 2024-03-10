using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class Trial : ProEstEntity
    {
        public int ID { get; private set; }
        public int LoginID { get; set; }
        public DateTime CreationStamp { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Active { get; set; }
        public bool HasPDR { get; set; }
        public bool HasEMS { get; set; }
        public bool HasFrameData { get; set; }
        public bool HasQBExport { get; set; }
        public bool HasProAdvisor { get; set; }
        public bool HasImages { get; set; }
        public bool HasCustomReports { get; set; }
        public bool HasBundle { get; set; }
        public bool IsDeleted { get; set; }

        public bool HasMultiUser { get; set; }

        private DateTime _databaseEndDate;

        public Trial()
        { }

        public Trial(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            CreationStamp = InputHelper.GetDateTime(row["CreationStamp"].ToString());
            StartDate = InputHelper.GetDateTime(row["StartDate"].ToString());
            EndDate = InputHelper.GetDateTime(row["EndDate"].ToString());
            _databaseEndDate = InputHelper.GetDateTime(row["EndDate"].ToString());
            Active = InputHelper.GetBoolean(row["Active"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());
            HasPDR = InputHelper.GetBoolean(row["HasPDR"].ToString());
            HasEMS = InputHelper.GetBoolean(row["HasEMS"].ToString());
            HasFrameData = InputHelper.GetBoolean(row["HasFrameData"].ToString());
            HasQBExport = InputHelper.GetBoolean(row["HasQBExport"].ToString());
            HasProAdvisor = InputHelper.GetBoolean(row["HasProAdvisor"].ToString());
            HasImages = InputHelper.GetBoolean(row["HasImages"].ToString());
            HasCustomReports = InputHelper.GetBoolean(row["HasCustomReports"].ToString());
            HasBundle = InputHelper.GetBoolean(row["HasBundle"].ToString());
            HasMultiUser = InputHelper.GetBoolean(row["HasMultiUser"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            int salesRepID = 0;

            if (activeLoginID > 0)
            {
                ActiveLogin activeLogin = ActiveLogin.Get(activeLoginID);
                salesRepID = activeLogin.SalesRepID;
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("CreationStamp", CreationStamp));
            parameters.Add(new SqlParameter("StartDate", StartDate));
            parameters.Add(new SqlParameter("EndDate", EndDate));
            parameters.Add(new SqlParameter("Active", Active));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));
            parameters.Add(new SqlParameter("HasPDR", HasPDR));
            parameters.Add(new SqlParameter("HasEMS", HasEMS));
            parameters.Add(new SqlParameter("HasFrameData", HasFrameData));
            parameters.Add(new SqlParameter("HasQBExport", HasQBExport));
            parameters.Add(new SqlParameter("HasProAdvisor", HasProAdvisor));
            parameters.Add(new SqlParameter("HasImages", HasImages));
            parameters.Add(new SqlParameter("HasCustomReports", HasCustomReports));
            parameters.Add(new SqlParameter("ModifiedBy", salesRepID));
            parameters.Add(new SqlParameter("HasBundle", HasBundle));
            parameters.Add(new SqlParameter("HasMultiUser", HasMultiUser));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Trial_Update", parameters);

            if (result.Success)
            {
                if (_databaseEndDate != EndDate && _databaseEndDate != DateTime.MinValue)
                {
                    ContractEndDateChangeLog log = new ContractEndDateChangeLog();
                    log.TrialID = ID;
                    log.OldDate = _databaseEndDate.Date;
                    log.NewDate = EndDate.Date;
                    log.SalesRepID = salesRepID;
                    log.Save();

                    _databaseEndDate = EndDate;
                }

                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "Trial", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static Trial Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("Trial_Get", new SqlParameter("ID", id));
            if (result.Success)
            {
                return new Trial(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static Trial GetActive(int loginID)
        {
            List<Trial> trials = GetForLogin(loginID);
            return GetActive(trials);
        }

        public static Trial GetActive(List<Trial> trials)
        {
            foreach (Trial trial in trials)
            {
                if (trial.Active && !trial.IsDeleted && trial.StartDate <= DateTime.Now.Date && trial.EndDate >= DateTime.Now.Date)
                {
                    return trial;
                }
            }

            return null;
        }

        public static List<Trial> GetForLogin(int loginID, bool showDeleted = false)
        {
            List<Trial> list = new List<Trial>();

            try
            {
                DBAccess db = new DBAccess();

                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("loginID", loginID));
                parameters.Add(new SqlParameter("showDeleted", showDeleted));

                DBAccessTableResult result = db.ExecuteWithTable("Trial_GetForLogin", parameters);
                if (result.Success)
                {
                    foreach (DataRow row in result.DataTable.Rows)
                    {
                        list.Add(new Trial(row));
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "Trial.GetForLogin");
            }      

            return list;
        }
    }
}
