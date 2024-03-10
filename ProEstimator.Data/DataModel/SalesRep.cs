using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ProEstimatorData.DataModel
{
    public class SalesRep
    {
        public int SalesRepID { get; set; }
        public string SalesNumber { get; set; }
        public string FirstName	{ get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Deleted { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneExtension { get; set; }
        public bool IsSalesRep { get; set; }
        public bool IsActive { get; set; }

        public SalesRep()
        {

        }

        public SalesRep(DataRow row)
        {
            SalesRepID = InputHelper.GetInteger(row["SalesRepID"].ToString());
            SalesNumber = InputHelper.GetString(row["SalesNumber"].ToString());
            FirstName = InputHelper.GetString(row["FirstName"].ToString());
            LastName = InputHelper.GetString(row["LastName"].ToString());
            Email = InputHelper.GetString(row["Email"].ToString());
            UserName = InputHelper.GetString(row["UserName"].ToString());
            Password = InputHelper.GetString(row["Password"].ToString());
            Deleted = InputHelper.GetBoolean(row["Deleted"].ToString());
            PhoneNumber = InputHelper.GetString(row["PhoneNumber"].ToString());
            PhoneExtension = InputHelper.GetString(row["PhoneExtension"].ToString());
            IsSalesRep = InputHelper.GetBoolean(row["isSalesRep"].ToString());
            IsActive = InputHelper.GetBoolean(row["Active"].ToString());
        }

        public static SalesRep Get(int salesRepID)
        {
            if (salesRepID < 0)
            {
                return null;
            }

            return GetAll().FirstOrDefault(o => o.SalesRepID == salesRepID);
        }

        public static List<SalesRep> GetAll()
        {
            lock(_loadLock)
            {
                if (_allSalesReps == null || _allSalesReps.Count == 0)
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult tableResult = db.ExecuteWithTable("GetAllSalesReps");

                    if (tableResult.Success)
                    {
                        _allSalesReps = new List<SalesRep>();

                        foreach (DataRow row in tableResult.DataTable.Rows)
                        {
                            _allSalesReps.Add(new SalesRep(row));
                        }
                    }
                }
            }            

            return _allSalesReps;
        }

        public static List<SalesRep> GetSpecial()
        {
            List<int> specialRepIDs = new List<int>() { 
                  100       // House
                , 101       // Shanks
                , 230       // Pam
                , 243       // Elena
                , 246       // Farrell
                , 340       // Lisa
                , 360       // Curtis
            };

            List<SalesRep> allReps = GetAll();
            List<SalesRep> specialReps = allReps.Where(o => specialRepIDs.Contains(InputHelper.GetInteger(o.SalesNumber))).ToList();
            return specialReps;
        }

        private void AddRepIfFound(List<SalesRep> allReps, List<SalesRep> filteredReps, int repID)
        {
            SalesRep rep = allReps.FirstOrDefault(o => o.SalesRepID == repID);
            if (rep != null)
            {
                filteredReps.Add(rep);
            }
        }

        public static void RefreshCache()
        {
            lock(_loadLock)
            {
                _allSalesReps = null;
            }            
        }

        private static object _loadLock = new object();
        private static List<SalesRep> _allSalesReps;

    }
}