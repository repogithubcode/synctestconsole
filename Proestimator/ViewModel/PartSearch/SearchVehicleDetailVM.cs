using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData;

namespace Proestimator.ViewModel.PartSearch
{
    public class SearchVehicleDetailVM
    {

        public string Make { get; private set; }
        public string Model { get; private set; }
        public int MinYear { get; private set; }
        public int MaxYear { get; private set; }

        public SearchVehicleDetailVM(DataRow row)
        {
            Make = InputHelper.GetString(row["Make"].ToString());
            Model = InputHelper.GetString(row["Model"].ToString());
            MinYear = InputHelper.GetInteger(row["MinYear"].ToString());
            MaxYear = InputHelper.GetInteger(row["MaxYear"].ToString());
        }

    }
}
