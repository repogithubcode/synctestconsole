using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class WebEstMonthlySalesVM
    {
        public List<SelectListItem> MonthSoldList { get; set; }
        public int SelectedMonth;

        public List<SelectListItem> YearSoldList { get; set; }
        public int SelectedYear;

        public List<MonthlySalesVM> MonthlySalesNew { get; set; }
        public List<MonthlySalesVM> MonthlySalesRenewal { get; set; }

        // summary fields
        public int CustCount { get; set; }
        public int RegContracts { get; set; }
        public int CommPkg { get; set; }
        public int Frame { get; set; }
        public int MTHCustCount { get; set; }
        public int YTDCustCount { get; set; }

        public decimal MTHTotalPrice { get; set; }
        public decimal YTDTotalPrice { get; set; }
        public decimal AVG1YTDTotalPrice { get; set; }
        public decimal AVG2YTDTotalPrice { get; set; }

        public WebEstMonthlySalesVM()
        {
            // MonthSoldList
            MonthSoldList = new List<SelectListItem>();
            MonthSoldList.Add(new SelectListItem { Text = "January", Value = "1" });
            MonthSoldList.Add(new SelectListItem { Text = "February", Value = "2" });
            MonthSoldList.Add(new SelectListItem { Text = "March", Value = "3" });
            MonthSoldList.Add(new SelectListItem { Text = "April", Value = "4" });
            MonthSoldList.Add(new SelectListItem { Text = "May", Value = "5" });
            MonthSoldList.Add(new SelectListItem { Text = "June", Value = "6" });
            MonthSoldList.Add(new SelectListItem { Text = "July", Value = "7" });
            MonthSoldList.Add(new SelectListItem { Text = "August", Value = "8" });
            MonthSoldList.Add(new SelectListItem { Text = "September", Value = "9" });
            MonthSoldList.Add(new SelectListItem { Text = "October", Value = "10" });
            MonthSoldList.Add(new SelectListItem { Text = "November", Value = "11" });
            MonthSoldList.Add(new SelectListItem { Text = "December", Value = "12" });

            this.MonthSoldList = MonthSoldList;

            // YearSoldList
            YearSoldList = new List<SelectListItem>();
            string yearStr = string.Empty;

            for (int year = 2015; year <= DateTime.Now.Year; year++)
            {
                yearStr = Convert.ToString(year);

                if(year == DateTime.Now.Year)
                    YearSoldList.Add(new SelectListItem { Text = yearStr, Value = yearStr,Selected=true });
                else
                    YearSoldList.Add(new SelectListItem { Text = yearStr, Value = yearStr });
            }

            this.YearSoldList = YearSoldList;
        }
    }
}