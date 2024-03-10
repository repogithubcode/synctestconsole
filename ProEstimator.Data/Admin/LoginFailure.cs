using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;
using ProEstimatorData.Admin.Model;

namespace ProEstimatorData
{
    public static class LoginFailure
    {
        public static List<VmLoginFailureReport> Get(string LoginNameFilter, string OrganizationFilter, string loginIdFilter)
        {
            VmLoginFailureReport vmLoginFailureReport = null;
            List<VmLoginFailureReport> vmLoginFailureReportListColl = new List<VmLoginFailureReport>();
            var result = new DataTable();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginNameFilter", LoginNameFilter));
            parameters.Add(new SqlParameter("OrganizationFilter", OrganizationFilter));

            DBAccessTableResult tableResult = db.ExecuteWithTable("getLoginFailures", parameters);

            if (tableResult != null && tableResult.DataTable != null)
            {
                foreach (DataRow eachDataRow in tableResult.DataTable.Rows)
                {
                    vmLoginFailureReport = new VmLoginFailureReport(eachDataRow);
                    vmLoginFailureReportListColl.Add(vmLoginFailureReport);
                }
            }

            return vmLoginFailureReportListColl.OrderByDescending(item => item.ID).ToList();
        }
    }
}