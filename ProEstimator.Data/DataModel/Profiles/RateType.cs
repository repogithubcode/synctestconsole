using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.Profiles
{
    public class RateType
    {
        public int ID { get; private set; }
        public string RateName	{ get; private set; }
        public string EMSCode1 { get; private set; }
        public string EMSSublet	{ get; private set; }

        public RateType(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            RateName = row["RateName"].ToString();
            EMSCode1 = row["EMSCode1"].ToString();
            EMSSublet = row["EMSSublet"].ToString();
        }

        private static object _loadLock = new object();
        private static List<RateType> _rateTypes;

        public static List<RateType> GetAll()
        {
            LoadTypes();
            return _rateTypes;
        }

        public static RateType GetByID(int rateTypeID)
        {
            LoadTypes();
            return _rateTypes.FirstOrDefault(o => o.ID == rateTypeID);
        }

        private static void LoadTypes()
        {
            lock(_loadLock)
            {
                if (_rateTypes == null || _rateTypes.Count == 0)
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult tableResult = db.ExecuteWithTableForQuery("SELECT id, RateName, EMSCode1, EMSSublet FROM RateTypes");

                    if (tableResult.Success)
                    {
                        _rateTypes = new List<RateType>();

                        foreach (DataRow row in tableResult.DataTable.Rows)
                        {
                            _rateTypes.Add(new RateType(row));
                        }
                    }
                    else
                    {
                        ErrorLogger.LogError("Error loading all Rate Types.", 0, 0, "Rate Types");
                    }
                }
            }
        }
    }
}
