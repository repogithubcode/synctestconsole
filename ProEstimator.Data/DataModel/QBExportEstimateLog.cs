using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class QBExportEstimateLog
    {

        public int ID { get; private set; }
        public int ExportLogID { get; set; }
        public int EstimateID { get; set; }
        public int Supplement { get; set; }

        public QBExportEstimateLog()
        {

        }

        public QBExportEstimateLog(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            ExportLogID = InputHelper.GetInteger(row["ExportLogID"].ToString());
            EstimateID = InputHelper.GetInteger(row["EstimateID"].ToString());
            Supplement = InputHelper.GetInteger(row["Supplement"].ToString());
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("ExportLogID", ExportLogID));
            parameters.Add(new SqlParameter("EstimateID", EstimateID));
            parameters.Add(new SqlParameter("Supplement", Supplement));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("QBExportEstimateLog_Update", parameters);
            if (intResult.Success)
            {
                ID = intResult.Value;
                return new SaveResult();
            }
            else
            {
                return new SaveResult(intResult.ErrorMessage);
            }
        }

        public static List<QBExportEstimateLog> GetByEstimateLog(int exportLogID)
        {
            List<QBExportEstimateLog> results = new List<QBExportEstimateLog>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("QBExportEstimateLog_Get", new SqlParameter("ExportLogID", exportLogID));

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                results.Add(new QBExportEstimateLog(row));
            }

            return results;
        }

    }
}
