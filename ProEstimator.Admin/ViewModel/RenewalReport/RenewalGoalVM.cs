using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.RenewalReport
{
    public class RenewalGoalVM
    {

        public bool GoodData { get; set; }
        public RenewalGoalDataVM RenewalGoal { get; set; }

        public string RenewalsTotalDisplay { get; set; }

        public List<BonusEarnedVM> BonusEarnedLines { get; set; }

        public List<RenewalDetailsVM> RenewalDetails { get; set; }

        public RenewalGoalVM(RenewalGoal renewalGoal, List<RenewalDetails> details)
        {
            RenewalDetails = new List<RenewalDetailsVM>();

            if (renewalGoal != null)
            {
                GoodData = true;

                RenewalGoal = new RenewalGoalDataVM(renewalGoal);

                foreach (RenewalDetails detail in details.OrderBy(o => o.ContractEnd))
                {
                    RenewalDetails.Add(new RenewalDetailsVM(detail));
                }

                // Due renewals for the first year, past the first year, and totals
                int dueRenewals1Year = details.Where(o => o.TotalYears <= 1).Count();
                int dueRenewals2Year = details.Where(o => o.TotalYears > 1).Count();
                int dueRenewalsTotal = details.Count();

                int newSales = renewalGoal.ActualSales;

                RenewalsTotalDisplay = dueRenewals1Year + "/" + dueRenewals2Year + "/" + dueRenewalsTotal;

                // Number of due renewals that already did renew, for first year, past first year, and total
                int year1Renewed = details.Where(o => o.TotalYears == 1 && o.DidRenew).Count();
                int year2Renewed = details.Where(o => o.TotalYears > 1 && o.DidRenew).Count();
                int totalRenewed = year1Renewed + year2Renewed;

                // MTD "Month To Date" totals - renewals up until today in the month
                int year1MTD = details.Where(o => o.TotalYears == 1 && o.DidRenew && o.ContractEnd <= DateTime.Now.Date).Count();
                int year2MTD = details.Where(o => o.TotalYears > 1 && o.DidRenew && o.ContractEnd <= DateTime.Now.Date).Count();
                int totalMTD = year1MTD + year2MTD;

                // Renewal goals.  
                int year1RenewedGoal = Convert.ToInt32(dueRenewals1Year * (RenewalGoal.RenewalGoal1Yr / 100.0));
                int year2RenewedGoal = Convert.ToInt32(dueRenewals2Year * (RenewalGoal.RenewalGoal2Yr / 100.0));
                int totalRenewalGoal = year1RenewedGoal + year2RenewedGoal;

                int salesGoal = RenewalGoal.SalesGoal;

                double totalRenewalPercentGoal = ((double)totalRenewalGoal / dueRenewalsTotal) * 100;

                double growthGoalCalculation = (double)(salesGoal + totalRenewalGoal) / dueRenewalsTotal;
                double growthGoalPercent = (growthGoalCalculation - 1) * 100;

                double growthActualCalculation = (totalRenewed + newSales) / (double)dueRenewalsTotal;
                double growthActualPercent = (growthActualCalculation - 1) * 100;

                // Actual sales
                double actualRenewalPercent = ((double)totalRenewed / dueRenewalsTotal) * 100;
                double actual1YearRenewal = ((double)year1Renewed / dueRenewals1Year) * 100;
                double actual2YearRenewal = ((double)year2Renewed / dueRenewals2Year) * 100;

                // MTD Percents
                double year1MTDPercent = ((double)year1MTD / dueRenewals1Year) * 100;
                double year2MTDPercent = ((double)year2MTD / dueRenewals2Year) * 100;
                double mtdPercentTotal = ((double)totalMTD / dueRenewalsTotal) * 100;

                double percentOfGoal = (growthActualPercent / growthGoalPercent) * 100;

                BonusEarnedLines = new List<BonusEarnedVM>();
                BonusEarnedLines.Add(new BonusEarnedVM("Renewal %", Percent(totalRenewalPercentGoal), "", Percent(actualRenewalPercent), "", Percent(mtdPercentTotal)));
                BonusEarnedLines.Add(new BonusEarnedVM("Renewed", totalRenewalGoal.ToString(), "", totalRenewed.ToString(), (year1MTD + year2MTD).ToString(), ""));
                BonusEarnedLines.Add(new BonusEarnedVM("Renewal 1 yr %", Percent(RenewalGoal.RenewalGoal1Yr), "", Percent(actual1YearRenewal), "", Percent(year1MTDPercent)));
                BonusEarnedLines.Add(new BonusEarnedVM("Renewed 1 yr", year1RenewedGoal.ToString(), "", year1Renewed.ToString(), year1MTD.ToString(), ""));
                BonusEarnedLines.Add(new BonusEarnedVM("Renewal 2 yr %", Percent(RenewalGoal.RenewalGoal2Yr), "", Percent(actual2YearRenewal), "", Percent(year2MTDPercent)));
                BonusEarnedLines.Add(new BonusEarnedVM("Renewed 2 yr", year2RenewedGoal.ToString(), "", year2Renewed.ToString(), year2MTD.ToString(), ""));
                BonusEarnedLines.Add(new BonusEarnedVM("Sales", salesGoal.ToString(), RenewalGoal.Forecast.ToString(), newSales.ToString(), "", ""));
                BonusEarnedLines.Add(new BonusEarnedVM("Growth %", Percent(growthGoalPercent), "", Percent(growthActualPercent), "", ""));
                BonusEarnedLines.Add(new BonusEarnedVM("Percent of Goal", "", "", Percent(percentOfGoal), "", ""));
                //BonusEarnedLines.Add(new BonusEarnedVM("Growth % Renewals", "", "", "", "", ""));
            }
            else
            {
                GoodData = false;
            }
        }

        private string Percent(double input)
        {
            return input.ToString("0.00") + "%";
        }
    }
}