using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.Models.SubModel;
using ProEstimatorData.DataModel;
using ProEstimatorData;
using ProEstimatorData.Models;

namespace Proestimator.ViewModel.PartSearch
{
    public class PartsSearchVM
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public string Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public List<PartSearchResult> Parts { get; set; }
        public bool CurrentVehicleOnly { get; set; }

        public SelectList YearsList { get; set; }
        public SelectList MakeList { get; set; }
        public SelectList ModelList { get; set; }

        public PartsSearchVM()
        {
            LoginID = 0;
            EstimateID = 0;

            YearsList = new SelectList(VehicleSearchManager.GetYearSimpleListItemList(), "Value", "Text");
            MakeList = new SelectList("");
            ModelList = new SelectList("");
            Parts = new List<PartSearchResult>();
        }

        public PartsSearchVM(int loginID, int estimateID)
        {
            LoginID = loginID;
            EstimateID = estimateID;

            YearsList = new SelectList(VehicleSearchManager.GetYearSimpleListItemList(), "Value", "Text");
            MakeList = new SelectList("");
            ModelList = new SelectList("");
            Parts = new List<PartSearchResult>();

            if (estimateID > 0)
            {
                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("sp_getVehicledetails", new System.Data.SqlClient.SqlParameter("AdminInfoId", estimateID));
 
                if (tableResult.Success)
                {
                    int year = InputHelper.GetInteger(tableResult.DataTable.Rows[0]["Year"].ToString());
                    int makeID = InputHelper.GetInteger(tableResult.DataTable.Rows[0]["MakeID"].ToString());
                    int modelID = InputHelper.GetInteger(tableResult.DataTable.Rows[0]["ModelID"].ToString());

                    Year = year.ToString();
                    Make = makeID.ToString();
                    Model = modelID.ToString();

                    if (year != 0)//0 means no vehicle and we want to use the initial MakeList and ModelList
                    {
                        MakeList = new SelectList(VehicleSearchManager.GetVehicleMakes(year), "Value", "Text");
                        ModelList = new SelectList(VehicleSearchManager.GetVehicleModels(year, makeID), "Value", "Text");
                    }
                }
            }
        }

    }
}