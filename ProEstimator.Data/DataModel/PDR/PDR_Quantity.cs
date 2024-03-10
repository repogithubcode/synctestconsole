using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ProEstimatorData.DataModel.PDR
{
    public class PDR_Quantity
    {

        public int ID { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }

        public PDR_Quantity(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Min = InputHelper.GetInteger(row["Min"].ToString());
            Max = InputHelper.GetInteger(row["Max"].ToString());
        }

        private static object _loadLock = new object();
        private static List<PDR_Quantity> _cache;

        public static List<PDR_Quantity> GetAll()
        {
            FillCache();
            return _cache;
        }

        public static PDR_Quantity GetByID(int id)
        {
            FillCache();
            return _cache.FirstOrDefault(o => o.ID == id);
        }

        private static void FillCache()
        {
            lock (_loadLock)
            {
                if (_cache == null)
                {
                    _cache = new List<PDR_Quantity>();

                    DBAccess db = new DBAccess();
                    DBAccessTableResult table = db.ExecuteWithTable("PDR_QuantityLookup_GetAll");

                    if (table != null && table.DataTable != null)
                    {
                        foreach (DataRow row in table.DataTable.Rows)
                        {
                            _cache.Add(new PDR_Quantity(row));
                        }
                    }
                }
            }
        }
    }
}
