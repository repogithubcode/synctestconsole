using System.Collections.Generic;
using System.Data;

namespace ProEstimatorData.DataModel.LinkRules
{
    /// <summary>
    /// For the admin page, wraps up details about results for a link rule.
    /// </summary>
    public class MatchDetails
    {
        public int MatchCountsTotal { get; set; }
        public List<MatchCount> MatchCounts { get; set; }
        public List<VehicleDetails> NoMatchVehicleDetails { get; set; }

        public MatchDetails()
        {
            MatchCounts = new List<MatchCount>();
            NoMatchVehicleDetails = new List<VehicleDetails>();
        }

        public MatchDetails(DataSet dataSet)
        {
            MatchCounts = new List<MatchCount>();
            NoMatchVehicleDetails = new List<VehicleDetails>();

            if (dataSet.Tables.Count > 0)
            {
                MatchCountsTotal = dataSet.Tables[0].Rows.Count;

                int max = dataSet.Tables[0].Rows.Count;
                if (max > 100)
                {
                    max = 100;
                }

                //foreach (DataRow row in dataSet.Tables[0].Rows)
                for (int i = 0; i < max; i++)
                {
                    MatchCounts.Add(new MatchCount(dataSet.Tables[0].Rows[i]));
                }
            }

            if (dataSet.Tables.Count > 1)
            {
                int max = dataSet.Tables[1].Rows.Count;
                if (max > 100)
                {
                    max = 100;
                }

                for (int i = 0; i < max; i++)
                {
                    NoMatchVehicleDetails.Add(new VehicleDetails(dataSet.Tables[1].Rows[i]));
                }
            }
        }
    }

    public class MatchCount
    {
        public string Category { get; set; }
        public string PartDescription { get; set; }
        public int VehicleCount { get; set; }

        public MatchCount(DataRow row)
        {
            Category = InputHelper.GetString(row["Category"].ToString());
            if (row.Table.Columns.Contains("PartDescription"))
            {
                PartDescription = InputHelper.GetString(row["PartDescription"].ToString());
            }
            else
            {
                PartDescription = "";
            }
            VehicleCount = InputHelper.GetInteger(row["VehicleCount"].ToString());
        }
    }

    public class VehicleDetails
    {
        public int ServiceBarcode { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int YearStart { get; set; }
        public int YearEnd { get; set; }

        public VehicleDetails(DataRow row)
        {
            ServiceBarcode = InputHelper.GetInteger(row["ServiceBarcode"].ToString());
            Make = InputHelper.GetString(row["Make"].ToString());
            Model = InputHelper.GetString(row["Model"].ToString());
            YearStart = InputHelper.GetInteger(row["YearStart"].ToString());
            YearEnd = InputHelper.GetInteger(row["YearEnd"].ToString());
        }
    }
}
