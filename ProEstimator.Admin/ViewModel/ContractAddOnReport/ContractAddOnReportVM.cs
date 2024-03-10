using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;
using ProEstimatorData;
using ProEstimator.Business.Model.Admin;
using ProEstimator.Business.Model.Admin.Reports;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel
{
    public class ContractAddOnReportVM
    {
        public int AddOnTypeID { get; set; }
        public string Type { get; set; }
        public int ContractCount { get; set; }

        public ContractAddOnReportVM() { }

        public ContractAddOnReportVM(ContractAddOnReport contractAddOnReport)
        {
            AddOnTypeID = InputHelper.GetInteger(contractAddOnReport.AddOnTypeID.ToString());
            Type = InputHelper.GetString(contractAddOnReport.Type.ToString());
            ContractCount = InputHelper.GetInteger(contractAddOnReport.ContractCount.ToString());
        }
    }
}