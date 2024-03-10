using ProEstimator.Business.Extension;
using ProEstimator.Business.Model;
using ProEstimatorData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Logic.Admin
{
    public class FeedBackBugReportService
    {
        public const string SalesReport_TABLE = "[Admin].[UserFeedBackBugReport]";

        /// <summary>
        ///Get Sales Board Report
        /// </summary>
        public DataTable GetUserFeedBackBugReportData(UserFeedBackVM reqObj)
        {
            var result = new DataTable();
            DBAccess db = new DBAccess();

            var item = db.ExecuteWithTable(SalesReport_TABLE, SearchParams(reqObj));
            if (item != null && item.DataTable != null)
            {
                item.DataTable.TableName = SalesReport_TABLE;
                return item.DataTable;
            }
            return result;
        }


        public List<UserFeedBackVM> GetUserFeedBackBugReport(UserFeedBackVM reqObj)
        {
            var dt = GetUserFeedBackBugReportData(reqObj);
            var result = (from DataRow row in dt.Rows select row.ToModel<UserFeedBackVM>()).ToList();

            result = result.OrderByDescending(item => item.CreatedDate).ToList();
            return result;
        }

        private List<SqlParameter> SearchParams(UserFeedBackVM requestObj)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@Name", GetParameterValue(requestObj.Name)) { SqlDbType = SqlDbType.VarChar, Size=50 });
            parameters.Add(new SqlParameter("@Module", GetParameterValue(requestObj.MainModule)) { SqlDbType = SqlDbType.VarChar, Size=50 });
            parameters.Add(new SqlParameter("@SubModule", GetParameterValue(requestObj.SubModule)) { SqlDbType = SqlDbType.VarChar, Size = 2000 });
            parameters.Add(new SqlParameter("@DateStart", GetParameterValue(requestObj.DateStart)) { SqlDbType = SqlDbType.Date });
            parameters.Add(new SqlParameter("@DateEnd", GetParameterValue(requestObj.DateEnd)) { SqlDbType = SqlDbType.Date });
            return parameters;
        }
        private dynamic GetParameterValue(object val)
        {
            if (val != null)
                return val;
            else
                return DBNull.Value;
        }
       
    }
}
