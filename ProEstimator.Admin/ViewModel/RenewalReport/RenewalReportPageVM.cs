using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.ViewModel.RenewalReport
{
    public class RenewalReportPageVM
    {
        public int SessionSalesRepID { get; set; }
        public bool GoodData { get; set; }
        public int LoginID { get; set; }

        public bool CanEditBonusGoals { get; set; }

        public string SalesRepName { get; set; }

        public int SalesRepID { get; set; }
        public SelectList SalesReps { get; set; }

        public bool IsAdmin { get; set; }

        public bool CanSelectSalesRep { get; set; }

        public int BonusMonthID { get; set; }
        public SelectList BonusMonths { get; set; }

        public int BonusYear { get; set; }
        public SelectList BonusYears { get; set; }

        public List<BonusEarnedVM> BonusEarnedLines { get; set; }

        public RenewalReportPageVM()
        {
            CanEditBonusGoals = true;

            // Sales Reps
            List<SelectListItem> salesRepItems = new List<SelectListItem>();

            List<ProEstimatorData.DataModel.SalesRep> salesReps = ProEstimatorData.DataModel.SalesRep.GetAll().Where(o => o.IsSalesRep && o.IsActive).ToList();

            salesRepItems.Insert(0,new SelectListItem() { Text = "Select", Value = "-2" });

            foreach (ProEstimatorData.DataModel.SalesRep salesRep in salesReps)
            {
                salesRepItems.Add(new SelectListItem() { Text = salesRep.FirstName + " " + salesRep.LastName, Value = salesRep.SalesRepID.ToString() });
            }

            salesRepItems.Insert(salesRepItems.Count, new SelectListItem() { Text = "View All", Value = "-1" });

            SalesReps = new SelectList(salesRepItems, "Value", "Text");

            // Bonus Months
            List<SelectListItem> months = new List<SelectListItem>();
            months.Add(new SelectListItem() { Text = "Select Month", Value = "0" });
            months.Add(new SelectListItem() { Text = "Jan", Value = "1" });
            months.Add(new SelectListItem() { Text = "Feb", Value = "2" });
            months.Add(new SelectListItem() { Text = "Mar", Value = "3" });
            months.Add(new SelectListItem() { Text = "Apr", Value = "4" });
            months.Add(new SelectListItem() { Text = "May", Value = "5" });
            months.Add(new SelectListItem() { Text = "Jun", Value = "6" });
            months.Add(new SelectListItem() { Text = "Jul", Value = "7" });
            months.Add(new SelectListItem() { Text = "Aug", Value = "8" });
            months.Add(new SelectListItem() { Text = "Sep", Value = "9" });
            months.Add(new SelectListItem() { Text = "Oct", Value = "10" });
            months.Add(new SelectListItem() { Text = "Nov", Value = "11" });
            months.Add(new SelectListItem() { Text = "Dec", Value = "12" });

            BonusMonths = new SelectList(months, "Value", "Text");

            // Bonus Years
            List<SelectListItem> years = new List<SelectListItem>();
            years.Add(new SelectListItem() { Text = "Select a Year", Value = "0" });
            for(int year = 2017; year < 2026; year++)
            {
                years.Add(new SelectListItem() { Text = year.ToString(), Value = year.ToString() });
            }
            BonusYears = new SelectList(years, "Value", "Text");

            // Set current year and month
            BonusYear = DateTime.Now.Year;

            BonusMonthID = DateTime.Now.Month;
            if (BonusMonthID == 0)
            {
                BonusMonthID = 12;
            }
        }

    }
}