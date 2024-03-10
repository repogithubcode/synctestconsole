using ProEstimatorData.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proestimator.ViewModel
{
    public class ReportsVM
    {
        public int LoginID { get; set; }
        public int EstimatorID { get; set; }

        public DateRange SalesReportRange { get; set; }
        public DateRange CustomerListReportRange { get; set; }
        public DateRange SupplierListReportRange { get; set; }
        public DateRange CloseRatioReportRange { get; set; }
        public SelectList EstimatorSelections { get; set; }

        public bool QBExportActive { get; set; }
        public bool ShowQBExport { get; set; }
        public string QBAddOnLink { get; set; }

        public bool includeClosedDeleted { get; set; }

        public bool SaveReportsHistorySalesReport { get; set; }
        public bool SaveReportsHistoryCustomerList { get; set; }
        public bool SaveReportsHistorySupplierList { get; set; }
        public bool SaveReportsHistorySavedCustomerList { get; set; }
        public bool SaveReportsHistoryCloseRatioReport { get; set; }  


        public int EstimateID { get; set; }

        public ReportsVM(int loginID, int activeLoginID)
        {
            LoginID = loginID;

            QBExportActive = false;

            SalesReportRange = new DateRange();
            CustomerListReportRange = new DateRange();
            SupplierListReportRange = new DateRange();
            CloseRatioReportRange = new DateRange();

            ReportFormat = new List<DropDownData>();
            ReportFormat.Add(new DropDownData("pdf", "Pdf"));
            ReportFormat.Add(new DropDownData("xlsx", "Excel"));

            // Fill the Estimators list
            List<SelectListItem> estimatorSelections = new List<SelectListItem>();
            estimatorSelections.Add(new SelectListItem() { Text = "All Estimators", Value = "0" });
            List<Estimator> estimators = Estimator.GetByLogin(LoginID, activeLoginID).OrderBy(o => o.FirstName + " " + o.LastName).ToList();
            if (estimators.Count > 0)
            {
                foreach (Estimator estimator in estimators)
                {
                    estimatorSelections.Add(new SelectListItem() { Text = estimator.FirstName + " " + estimator.LastName, Value = estimator.ID.ToString() });
                }
            }
            EstimatorSelections = new SelectList(estimatorSelections, "Value", "Text");
            EstimatorID = 0;
        }

        public List<DropDownData> ReportFormat { get; set; }
    }

    public class DateRange
    {
        public string DateStart { get; set; }
        public string DateEnd { get; set; }

        public DateTime GetStartDate()
        {
            return ParseDate(DateStart);
        }

        public DateTime GetEndDate()
        {
            return ParseDate(DateEnd);
        }

        private DateTime ParseDate(string dateString)
        {
            DateTime result = DateTime.Now;

            try
            {
                result = DateTime.Parse(dateString);
            }
            catch { }

            return result;
        }
    }
}