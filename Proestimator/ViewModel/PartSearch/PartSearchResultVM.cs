using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData;

namespace Proestimator.ViewModel.PartSearch
{
    public class PartSearchResultVM
    {

        public string ServiceBarcode { get; private set; }
        public string Category { get; private set; }
        public string Subcategory { get; private set; }
        public string PartDescription { get; private set; }
        public string PartNumber { get; private set; }
        public string DetailDescription { get; private set; }
        public double Price { get; private set; }

        public string Make { get; private set; }
        public string Model { get; private set; }
        public int MinYear { get; private set; }
        public int MaxYear { get; private set; }
        public int TotalVehicles { get; private set; }

        public PartSearchResultVM(DataRow row)
        {
            ServiceBarcode = InputHelper.GetString(row["Service_BarCode"].ToString());
            Category = InputHelper.GetString(row["Category"].ToString());
            Subcategory = InputHelper.GetString(row["Subcategory"].ToString());
            PartDescription = InputHelper.GetString(row["Part_Desc"].ToString());
            PartNumber = InputHelper.GetString(row["Part_Number"].ToString());
            DetailDescription = InputHelper.GetString(row["Prtc_Description"].ToString());
            Price = InputHelper.GetDouble(row["Price_1"].ToString()) / 100;

            Make = InputHelper.GetString(row["Make"].ToString());
            Model = InputHelper.GetString(row["Model"].ToString());
            MinYear = InputHelper.GetInteger(row["MinYear"].ToString());
            MaxYear = InputHelper.GetInteger(row["MaxYear"].ToString());
            TotalVehicles = InputHelper.GetInteger(row["TotalVehicles"].ToString());
        }

    }
}