using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.MonthlySalesDashboard
{
    public class PayCodeCommissionVM
    {
        public int PayCodeCommissionID { get; set; }

        public bool HasEMS { get; set; }
        public bool HasFrame { get; set; }
        public bool HasWT { get; set; }

        public string ContractTermDescription { get; set; }
        public int PayCode { get; set; }
        public double Commission { get; set; }

        public bool HasMU { get; set; }
        public double MUCommission { get; set; }

        public bool HasQB { get; set; }
        public double QBCommission { get; set; }

        public string CodeListVsText { get; set; }
        public string WEText { get; set; }

        public string SalesType { get; set; }

        public PayCodeCommissionVM(PayCodeCommission PayCodeCommission)
        {
            this.PayCodeCommissionID = PayCodeCommission.PayCodeCommissionID;

            this.HasEMS = PayCodeCommission.HasEMS;
            this.HasFrame = PayCodeCommission.HasFrame;
            this.HasWT = PayCodeCommission.HasWT;

            this.ContractTermDescription = PayCodeCommission.ContractTermDescription;
            this.PayCode = PayCodeCommission.PayCode;
            this.Commission = PayCodeCommission.Commission;

            this.HasMU = PayCodeCommission.HasMU;
            this.MUCommission = PayCodeCommission.MUCommission;

            this.HasQB = PayCodeCommission.HasQB;
            this.QBCommission = PayCodeCommission.QBCommission;

            this.CodeListVsText = PayCodeCommission.CodeListVsText;
            this.WEText = PayCodeCommission.WEText;

            this.SalesType = PayCodeCommission.SalesType;
        }
    }
}
