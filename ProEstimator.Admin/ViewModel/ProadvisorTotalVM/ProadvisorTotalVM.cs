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
    public class ProadvisorTotalVM
    {
        public int ID { get; set; }
        public int LoginId { get; set; }
        public string LoginName { get; set; }
        public string CompanyName { get; set; }
        public SelectList SalesRepDDL { get; set; }
        public string SalesRep { get; set; }
        public Double ProAdvisorEstimateTotal { get; set; }
        public string EstimationDate { get; set; }

        public ProadvisorTotalVM() { }

        public ProadvisorTotalVM(ProadvisorTotal proadvisorTotalReport)
        {
            LoginId = InputHelper.GetInteger(proadvisorTotalReport.LoginId.ToString());
            CompanyName = proadvisorTotalReport.CompanyName;
            SalesRep = proadvisorTotalReport.SalesRep;
            ProAdvisorEstimateTotal = InputHelper.GetDouble(proadvisorTotalReport.ProAdvisorEstimateTotal.ToString());
            EstimationDate = proadvisorTotalReport.EstimationDate.ToString("MM/dd/yyyy HH:mm tt");
        }
    }
}