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
    public class SalesBoardDetailReportVM
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int Frame { get; set; }
        public int Ems { get; set; }
        public int MultiUser { get; set; }
        public string SalesRepID { get; set; }
        public string CompanyName { get; set; }

        public string Month { get; set; }
        public string Year { get; set; }

        public int Bundle { get; set; }
        public int QBExporter { get; set; }
        public int ProAdvisor { get; set; }
        public int ImageEditor { get; set; }
        public int EnterpriseReporting { get; set; }

        public SalesBoardDetailReportVM(SalesBoardDetailReport salesBoardDetailReport, string month, string year)
        {
            Name = salesBoardDetailReport.Name;
            Count = salesBoardDetailReport.Count;
            Frame = salesBoardDetailReport.Frame;
            Ems = salesBoardDetailReport.Ems;
            MultiUser = salesBoardDetailReport.MultiUser;
            Bundle = salesBoardDetailReport.Bundle;
            QBExporter = salesBoardDetailReport.QBExporter;
            ProAdvisor = salesBoardDetailReport.ProAdvisor;
            ImageEditor = salesBoardDetailReport.ImageEditor;
            EnterpriseReporting = salesBoardDetailReport.EnterpriseReporting;

            SalesRepID = salesBoardDetailReport.SalesRepID;
            Month = month;
            Year = year;
        }
    }
}


