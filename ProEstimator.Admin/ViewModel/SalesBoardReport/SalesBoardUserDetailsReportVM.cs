using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using System.Data;
using Newtonsoft.Json;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.SalesBoard
{
    public class SalesBoardUserDetailsReportVM
    {
        public int SalesBoardID { get; set; }
        public string Name { get; set; }
        public int? SalesBoardId { get; set; }
        public int? LoginId { get; set; }
        public int? NumberSold { get; set; } // Estimatics
        public int? Frame { get; set; }
        public int? Ems { get; set; } // Com Pkg
        public string CompanyName { get; set; }
        public string MultiUser { get; set; }
        public string DateSold { get; set; }

        public string HasBundle { get; set; }
        public string HasQBExporter { get; set; }
        public string HasProAdvisor { get; set; }
        public string HasImages { get; set; }
        public string HasReporting { get; set; }

        public SalesBoardUserDetailsReportVM()
        {

        }

        public SalesBoardUserDetailsReportVM(SalesBoardUserDetailsReport salesBoardUserDetailsReport)
        {
            SalesBoardID = salesBoardUserDetailsReport.SalesBoardID;
            Name = salesBoardUserDetailsReport.Name;
            SalesBoardId = salesBoardUserDetailsReport.SalesBoardID;
            LoginId = salesBoardUserDetailsReport.LoginId;
            CompanyName = salesBoardUserDetailsReport.CompanyName;

            NumberSold = salesBoardUserDetailsReport.NumberSold;
            Frame = salesBoardUserDetailsReport.Frame;
            Ems = salesBoardUserDetailsReport.Ems;
            if (salesBoardUserDetailsReport.AddUser == 0)
                MultiUser = "No";
            else
                MultiUser = "Yes";

            HasBundle = salesBoardUserDetailsReport.HasBundle == 0 ? "No" : "Yes";
            HasQBExporter = salesBoardUserDetailsReport.HasQBExporter == 0 ? "No" : "Yes";
            HasProAdvisor = salesBoardUserDetailsReport.HasProAdvisor == 0 ? "No" : "Yes";
            HasImages = salesBoardUserDetailsReport.HasImages == 0 ? "No" : "Yes";
            HasReporting = salesBoardUserDetailsReport.HasReporting == 0 ? "No" : "Yes";

            DateSold = salesBoardUserDetailsReport.DateSold;

        }
    }
}


