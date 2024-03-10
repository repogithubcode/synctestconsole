using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using System.Data;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace ProEstimator.Admin.ViewModel.SalesBoard
{
    public class SalesBoardVM 
    {
        public SelectList SalesRepDDL { get; set; }
        public string SelectedSalesRep { get; set; }

        public SelectList MonthSoldDDL { get; set; }
        public string SelectedMonthSold { get; set; }

        public SelectList YearSoldDDL { get; set; }
        public string SelectedYearSold { get; set; }

        public bool HasCommitPermission { get; set; }

        public SalesBoardVM()
        {
            
        }
    }
}


