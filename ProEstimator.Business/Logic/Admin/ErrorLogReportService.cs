using ProEstimator.Business.Model.Admin;
using ProEstimatorData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ProEstimator.Business.Extension;
using System.Threading.Tasks;
using ProEstimator.Business.Model.Admin.Reports;

namespace ProEstimator.Business.Logic.Admin
{
    public class ErrorLogReportService
    {
        public const string ErrorLogReport_SP = "Error_GetList";

        private List<SqlParameter> ErrorLogSearchParams(ErrorLogReportRequest requestObj)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@LoginID", GetParameterValue(requestObj.LoginID)) { SqlDbType=SqlDbType.Int });
            parameters.Add(new SqlParameter("@AdminInfoID", GetParameterValue(requestObj.AdminInfoID)) { SqlDbType = SqlDbType.Int });
            parameters.Add(new SqlParameter("@ErrorTag", GetParameterValue(requestObj.ErrorTag)) { SqlDbType = SqlDbType.VarChar, Size=255 });
            parameters.Add(new SqlParameter("@RangeStart", GetParameterValue(requestObj.RangeStart)) { SqlDbType = SqlDbType.Date });
            parameters.Add(new SqlParameter("@RangeEnd", GetParameterValue(requestObj.RangeEnd)) { SqlDbType = SqlDbType.Date });
            return parameters;
        }
       private dynamic GetParameterValue(object val)
        {
            if (val != null)
                return val;
            else
                return DBNull.Value;
        }
        /// <summary>
        ///Get ErrorLogs
        /// </summary>
        private DataTable GetErrorLogs(ErrorLogReportRequest requestObj)
        {
            var result = new DataTable();
            DBAccess db = new DBAccess();
            
            var item = db.ExecuteWithTable(ErrorLogReport_SP, ErrorLogSearchParams(requestObj));
            if (item != null && item.DataTable != null)
            {
                item.DataTable.TableName = ErrorLogReport_SP;
                return item.DataTable;
            }
            return result;
        }

        /// <summary>
        ///Get ErrorLog Report 
        /// </summary>
        public List<VmErrorLogReport> GetErrorLogsReport(ErrorLogReportRequest requestObj)
        {
            var dt = GetErrorLogs(requestObj);
            var result = (from DataRow row in dt.Rows
                          select row.ToModel<VmErrorLogReport>()).ToList();

            result = result.OrderByDescending(item => item.LoginID).ToList();
            return result;
        }
    }
}
