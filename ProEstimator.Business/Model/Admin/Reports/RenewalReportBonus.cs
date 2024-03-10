using ProEstimator.Business.ILogic;
using System.Data;

using ProEstimatorData;
using ProEstimator.Business.Extension;

namespace ProEstimator.Business.Model.Admin.Reports
{
    public class RenewalReportBonus : IModelMap<RenewalReportBonus>
    {
        public int Id { get; set; }
        public int SalesRepId { get; set; }
        public int BonusMonth { get; set; }
        public int BonusYear { get; set; }
        public int RenewalGoal1Yr { get; set; }
        public int RenewalGoal2Yr { get; set; }
        public int SalesGoal { get; set; }
        public int SalesBonus100 { get; set; }
        public int SalesBonus110 { get; set; }
        public int SalesBonus120 { get; set; }
        public int SalesBonus130 { get; set; }
        public int RenewalBonus1Yr100 { get; set; }
        public int RenewalBonus1Yr110 { get; set; }
        public int RenewalBonus120 { get; set; }
        public int RenewalBonus130 { get; set; }
        public string CreateDate { get; set; }
        public string ModifiedDate { get; set; }
        public int Forecast { get; set; }
        public int ActualSales { get; set; }

        public RenewalReportBonus ToModel(DataRow row)
        {
            var model = new RenewalReportBonus();
            if (row.Table.Columns.Contains("ID"))
            {
                model.Id = (int)row["ID"];
            }
            if (row.Table.Columns.Contains("SalesRepId"))
            {
                model.SalesRepId = (int)row["SalesRepId"];
            }
            if (row.Table.Columns.Contains("BonusMonth"))
            {
                model.BonusMonth = (int)row["BonusMonth"];
            }
            if (row.Table.Columns.Contains("BonusYear"))
            {
                model.BonusYear = (int)row["BonusYear"];
            }
            model.RenewalGoal1Yr = InputHelper.GetInteger(row["RenewalGoal1Yr"].ToString());
            model.RenewalGoal2Yr = InputHelper.GetInteger(row["RenewalGoal2Yr"].ToString());
            model.SalesGoal = InputHelper.GetInteger(row["SalesGoal"].ToString());
            model.SalesBonus100 = InputHelper.GetInteger(row["SalesBonus100"].ToString());
            model.SalesBonus110 = InputHelper.GetInteger(row["SalesBonus110"].ToString());
            model.SalesBonus120 = InputHelper.GetInteger(row["SalesBonus120"].ToString());
            model.SalesBonus130 = InputHelper.GetInteger(row["SalesBonus130"].ToString());
            model.RenewalBonus1Yr100 = InputHelper.GetInteger(row["RenewalBonus1Yr100"].ToString());
            model.RenewalBonus1Yr110 = InputHelper.GetInteger(row["RenewalBonus1Yr110"].ToString());
            model.RenewalBonus120 = InputHelper.GetInteger(row["RenewalBonus120"].ToString());
            model.RenewalBonus130 = InputHelper.GetInteger(row["RenewalBonus130"].ToString());
            model.Forecast = InputHelper.GetInteger(row["SalesForcast"].ToString());


            if (row.Table.Columns.Contains("CreateDate"))
            {
                model.CreateDate = row["CreateDate"].SafeDate();
            }
            if (row.Table.Columns.Contains("ModifiedDate"))
            {
                model.ModifiedDate = row["ModifiedDate"].SafeDate();
            }

            return model;
        }
    }
}
