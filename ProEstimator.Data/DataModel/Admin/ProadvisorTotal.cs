using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class ProadvisorTotal
    {
        public int LoginId { get; set; }
        public string LoginName { get; set; }
        public string CompanyName { get; set; }
        public string SalesRep { get; set; }
        public Double ProAdvisorEstimateTotal { get; set; }
        public DateTime EstimationDate { get; set; }

        public ProadvisorTotal()
        {

        }

        public ProadvisorTotal(DataRow row)
        {
            LoginId = InputHelper.GetInteger(row["LoginID"].ToString());
            LoginName = InputHelper.GetString(row["LoginName"].ToString());
            CompanyName = InputHelper.GetString(row["CompanyName"].ToString());
            SalesRep = InputHelper.GetString(row["SalesRepFullName"].ToString());
            ProAdvisorEstimateTotal = InputHelper.GetDouble(row["ProAdvisorEstimateTotal"].ToString());
            EstimationDate = InputHelper.GetDateTime(row["EstimationDate"].ToString());
        }

        public static List<ProadvisorTotal> GetForFilter(int loginIDFilter, string organizationFilter, int salesrepFilter,
                                                                                string rangeStartFilter = null, string rangeEndFilter = null)
        {
            List<ProadvisorTotal> proadvisorTotals = new List<ProadvisorTotal>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            if(loginIDFilter > 0)
                parameters.Add(new SqlParameter("LoginID", loginIDFilter));

            if (!string.IsNullOrEmpty(organizationFilter))
                parameters.Add(new SqlParameter("CompanyName", organizationFilter));

            if (salesrepFilter > 0)
                parameters.Add(new SqlParameter("SalesRepID", salesrepFilter));

            if (!string.IsNullOrEmpty(rangeStartFilter))
                parameters.Add(new SqlParameter("RangeStart", rangeStartFilter));

            if (!string.IsNullOrEmpty(rangeEndFilter))
                parameters.Add(new SqlParameter("RangeEnd", rangeEndFilter));

            DBAccessTableResult tableResult = db.ExecuteWithTable("ProAdvisorEstimateTotalPerCustomer", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                proadvisorTotals.Add(new ProadvisorTotal(row));
            }

            return proadvisorTotals.OrderByDescending(item => item.SalesRep).ToList();
        }
    }
}