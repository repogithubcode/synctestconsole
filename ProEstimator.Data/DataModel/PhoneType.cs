using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class PhoneType
    {

        public int ID { get; private set; }
        public string Code { get; private set; }
        public string ScreenDisplay { get; private set; }
        public string ReportDisplay { get; private set; }

        private static object _loadLock = new object();
        private static List<PhoneType> _cache;

        public PhoneType(string code, string display)
        {
            ID = 0;
            Code = code;
            ScreenDisplay = display;
            ReportDisplay = display;
        }

        public PhoneType(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Code = row["Code"].ToString();
            ScreenDisplay = row["Screen"].ToString();
            ReportDisplay = row["Report"].ToString();
        }

        public static PhoneType GetByID(int id)
        {
            CreateCache();
            return _cache.FirstOrDefault(o => o.ID == id);
        }

        public static PhoneType GetByCode(string code)
        {
            CreateCache();
            return _cache.FirstOrDefault(o => o.Code == code);
        }

        public static List<PhoneType> GetAll()
        {
            CreateCache();
            return _cache.ToList();
        }

        private static void CreateCache()
        {
            lock(_loadLock)
            {
                if (_cache == null || _cache.Count == 0)
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult table = db.ExecuteWithTable("PhoneTypes_GetAll");
                    if (table.Success)
                    {
                        _cache = new List<PhoneType>();

                        foreach (DataRow row in table.DataTable.Rows)
                        {
                            _cache.Add(new PhoneType(row));
                        }
                    }
                    else
                    {
                        ErrorLogger.LogError(table.ErrorMessage, 0, 0, "PhoneType Error in CreateCache");
                    }
                }
            }
        }

    }
}
