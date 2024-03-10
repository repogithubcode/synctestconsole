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
    public class SalesBoardReportVM 
    {
        public int Year { get; set; }
        public int Jan { get; set; }
        public int Feb { get; set; }
        public int Mar { get; set; }
        public int Apr { get; set; }
        public int May { get; set; }
        public int Jun { get; set; }
        public int Jul { get; set; }
        public int Aug { get; set; }
        public int Sept { get; set; }
        public int Oct { get; set; }
        public int Nov { get; set; }
        public int Dec { get; set; }
        public int? Total { get; set; }

        public SalesBoardReportVM(SalesBoardReport salesBoardReport)
        {
            Year = salesBoardReport.Year;
            Jan = salesBoardReport.Jan;
            Feb = salesBoardReport.Feb;
            Mar = salesBoardReport.Mar;
            Apr = salesBoardReport.Apr;
            May = salesBoardReport.May;
            Jun = salesBoardReport.Jun;
            Jul = salesBoardReport.Jul;
            Aug = salesBoardReport.Aug;
            Sept = salesBoardReport.Sept;
            Oct = salesBoardReport.Oct;
            Nov = salesBoardReport.Nov;
            Dec = salesBoardReport.Dec;
            Total = salesBoardReport.Total;
        }
    }
}


