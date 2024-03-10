using ProEstimatorData.DataModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimator.Admin.ViewModel.RenewalReport
{
    public class RenewalGoalDataVM
    {
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
        public DateTime? CreateDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int Forecast { get; set; }
        public int ActualSales { get; set; }

        public RenewalGoalDataVM()
        {

        }

        public RenewalGoalDataVM(RenewalGoal renewalGoal)
        {
            BonusMonth = renewalGoal.BonusMonth;
            BonusYear = renewalGoal.BonusYear;
            RenewalGoal1Yr = renewalGoal.RenewalGoal1Yr;
            RenewalGoal2Yr = renewalGoal.RenewalGoal2Yr;
            SalesGoal = renewalGoal.SalesGoal;
            SalesBonus100 = renewalGoal.SalesBonus100;
            SalesBonus110 = renewalGoal.SalesBonus110;
            SalesBonus120 = renewalGoal.SalesBonus120;
            SalesBonus130 = renewalGoal.SalesBonus130;
            RenewalBonus1Yr100 = renewalGoal.RenewalBonus1Yr100;
            RenewalBonus1Yr110 = renewalGoal.RenewalBonus1Yr110;
            RenewalBonus120 = renewalGoal.RenewalBonus120;
            RenewalBonus130 = renewalGoal.RenewalBonus130;
            CreateDate = renewalGoal.CreateDate;
            ModifiedDate = renewalGoal.ModifiedDate;
            Forecast = renewalGoal.Forecast;
            ActualSales = renewalGoal.ActualSales;
        }
    }
}