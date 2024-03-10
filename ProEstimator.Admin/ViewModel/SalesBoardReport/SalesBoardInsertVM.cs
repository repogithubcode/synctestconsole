using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using System.Data;
using Newtonsoft.Json;

namespace ProEstimator.Admin.ViewModel.SalesBoard
{
    public class SalesBoardInsertVM
    {
        public string LoginID { get; set; }
        public string Est { get; set; }
        public string Frame { get; set; }
        public string EMS { get; set; }
        public string SalesRep { get; set; }
        public string SoldMonth { get; set; }
        public string SoldYear { get; set; }
        public string AddUser { get; set; }
        public string DateSold { set; get; }
        public string HasQBExporter { set; get; }
        public string ProAdvisor { get; set; }
        public string HasBundle { get; set; }
        public string HasImageEditor { get; set; }
        public string HasEnterpriseReporting { get; set; }
    }
}


