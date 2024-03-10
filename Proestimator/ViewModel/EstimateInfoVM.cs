using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using Proestimator;
using ProEstimatorData;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class EstimateInfoVM
    {
        public int EstimateNumber { get; private set; }
        public int AdminInfoID { get; private set; }
        public string Description { get; private set; }
        public DateTime DateCreated { get; private set; }
        public string Name { get; private set; }
        public string BusinessName { get; private set; }
        public string Vehicle { get; private set; }
        public string Status { get; private set; }
        public string TotalCost { get; private set; }
        public DateTime LastView { get; private set; }
        public int ImageCount { get; private set; }
        public bool Imported { get; private set; }
        public string RepairOrderNumber { get; private set; }

        public EstimateInfoVM(DataRow row)
        {
            EstimateNumber = InputHelper.GetInteger(row["EstimateNumber"].ToString());
            AdminInfoID = InputHelper.GetInteger(row["id"].ToString());
            Description = row["Description"].ToString();
            DateCreated = InputHelper.GetDateTime(row["DateCreated"].ToString());
            Name = row["Name"].ToString();
            BusinessName = row["BusinessName"].ToString();
            Vehicle = row["Vehicle"].ToString();
            Status = row["Status"].ToString();
            TotalCost = InputHelper.GetDouble(row["TotalCost"].ToString()).ToString("C");
            LastView = InputHelper.GetDateTime(row["LastView"].ToString());
            ImageCount = InputHelper.GetInteger(row["ImageCount"].ToString());
            Imported = InputHelper.GetBoolean(row["Imported"].ToString());
            RepairOrderNumber = row["RepairOrderNumber"].ToString();

            if (RepairOrderNumber == "0")
            {
                RepairOrderNumber = "";
            }
        }
    }

    
}