using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.ViewModel.MonthlySalesDashboard
{
    public class MonthlySalesDashboardPageVM
    {
        public int SessionSalesRepID { get; set; }
        public bool GoodData { get; set; }
        public int LoginID { get; set; }
        public bool IsAdmin { get; set; }

        public int CurrentMonth { get; set; }
        public SelectList MonthList { get; set; }

        public int CurrentYear { get; set; }
        public SelectList YearList { get; set; }

        public MonthlySalesDashboardPageVM()
        {
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

            MonthList = new SelectList(months, "Value", "Text");

            // Bonus Years
            List<SelectListItem> years = new List<SelectListItem>();
            years.Add(new SelectListItem() { Text = "Select a Year", Value = "0" });
            for(int year = 2017; year < 2026; year++)
            {
                years.Add(new SelectListItem() { Text = year.ToString(), Value = year.ToString() });
            }
            YearList = new SelectList(years, "Value", "Text");

            // Set current year and month
            CurrentYear = DateTime.Now.Year;
            CurrentMonth = DateTime.Now.Month;
        }
    }
}