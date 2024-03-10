using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;
using ProEstimatorData.Models.SubModel;

namespace ProEstimatorData
{
    public static class VehicleSearchManager
    {

        public static IEnumerable<SimpleListItem> GetYearSimpleListItemList()
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SP_GetYears");

            List<SimpleListItem> results = new List<SimpleListItem>();
            results.Add(new SimpleListItem() { Value = "0", Text = "-----Select Year-----" });

            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    results.Add(new SimpleListItem() { Value = row["id"].ToString(), Text = row["ModelYear"].ToString() });
                }
            }

            return results;
        }

        //public static SelectList GetYearsSelectList()
        //{
        //    return new SelectList(GetYearSimpleListItemList(), "Value", "Text");
        //}

        public static IEnumerable<SimpleListItem> GetVehicleMakes(int year)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SP_GetMakes", new SqlParameter("Year", year));

            List<SimpleListItem> results = new List<SimpleListItem>();
            results.Add(new SimpleListItem() { Value = "0", Text = "-----Select Make-----" });

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    results.Add(new SimpleListItem() { Value = row["MakeId"].ToString(), Text = row["Make"].ToString() });
                }
            }

            return results;
        }

        //public static SelectList GetVehicleMakesSelectList(int year)
        //{
        //    return new SelectList(GetVehicleMakes(year), "Value", "Text");
        //}

        public static IEnumerable<SimpleListItem> GetVehicleModels(int year, int makeid)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Year", year));
            parameters.Add(new SqlParameter("MakeID", makeid));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SP_GetModels", parameters);

            List<SimpleListItem> results = new List<SimpleListItem>();
            results.Add(new SimpleListItem() { Value = "0", Text = "-----Select Model-----" });

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    results.Add(new SimpleListItem() { Value = row["ModelId"].ToString(), Text = row["Model"].ToString() });
                }
            }

            return results;
        }

        //public static SelectList GetVehicleModelsSelectList(int year, int makeid)
        //{
        //    return new SelectList(GetVehicleModels(year, makeid), "Value", "Text");
        //}

    }
}