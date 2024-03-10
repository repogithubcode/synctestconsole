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
    public class SalesBoardAllVM
    {
        public string Name { get; set; }
        public int? SalesBoardID { get; set; }
        public int? LoginID { get; set; }
        public int? NumberSold { get; set; }
        public string DateSold { get; set; }
        public int? Frame { get; set; }
        public int? Ems { get; set; }
        public string CompanyName { get; set; }

        public SalesBoardAllVM(DataRow row)
        {
            Name = row["Name"].SafeString();
            SalesBoardID = row["salesBoardID"].SafeInt();
            LoginID = row["loginID"].SafeInt();
            NumberSold = row["numberSold"].SafeInt();
            DateSold = row["dateSold"].SafeDate();
            Frame = row["frame"].SafeInt();
            Ems = row["ems"].SafeInt();
        }
    }
}


