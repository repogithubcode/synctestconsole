using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class ReportHeader
    {

        public int ID { get; private set; }
        public string Header { get; private set; }
        public string Type { get; private set; }

        public ReportHeader(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            Header = row["Header"].ToString();
            Type = row["ReportType"].ToString();
        }

        private static object _loadLock = new object();
        private static List<ReportHeader> _reportHeaders;

        public static List<ReportHeader> ReportHeadersList
        {
            get
            {
                lock(_loadLock)
                {
                    if (_reportHeaders == null || _reportHeaders.Count == 0)
                    {
                        DBAccess db = new DBAccess();
                        DBAccessTableResult tableResult = db.ExecuteWithTable("GetReportHeaders");
                        if (tableResult.Success)
                        {
                            _reportHeaders = new List<ReportHeader>();

                            foreach (DataRow row in tableResult.DataTable.Rows)
                            {
                                _reportHeaders.Add(new ReportHeader(row));
                            }
                        }
                    }
                }

                return _reportHeaders;
            }
        }
    }
}
