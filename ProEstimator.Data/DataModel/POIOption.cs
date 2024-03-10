using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class POIOption
    {

        public int ID { get; private set; }
        public string Name { get; private set; }

        public POIOption(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
        }

        private static List<POIOption> _poiOptions;
        private static object _poiOptionsLock = new object();

        public static List<POIOption> GetAll()
        {
            lock(_poiOptionsLock)
            {
                if (_poiOptions == null)
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult tableResult = db.ExecuteWithTable("POIOptions_GetAll");

                    if (tableResult.Success)
                    {
                        _poiOptions = new List<POIOption>();

                        foreach(DataRow row in tableResult.DataTable.Rows)
                        {
                            _poiOptions.Add(new POIOption(row));
                        }
                    }
                }
            }

            return _poiOptions.ToList();
        }
    }

}
