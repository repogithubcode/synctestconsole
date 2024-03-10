using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class POISection
    {

        public int ID { get; private set; }
        public string Name { get; private set; }

        public POISection(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
        }

        private static List<POISection> _poiSections;
        private static object _poiSectionsLock = new object();

        public static List<POISection> GetAll()
        {
            lock (_poiSectionsLock)
            {
                if (_poiSections == null)
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult tableResult = db.ExecuteWithTable("POILabels_GetAll");

                    if (tableResult.Success)
                    {
                        _poiSections = new List<POISection>();

                        foreach (DataRow row in tableResult.DataTable.Rows)
                        {
                            _poiSections.Add(new POISection(row));
                        }
                    }
                }
            }

            return _poiSections.ToList();
        }
    }

}
