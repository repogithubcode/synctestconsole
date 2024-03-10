using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Mvc;
using ProEstimatorData.Models.SubModel;

namespace Proestimator.ViewModel.Printing
{
    public class WorkOrderReportVM
    {
        public Boolean Select { get; set; }

        public int LaborTypeID { get; set; }
        public string LaborTypeText { get; set; }
        public List<TechnicianVM> TechnicianVMList { get; set; }

        public int SelectedTechnicianID { get; set; }

        public WorkOrderReportVM()
        {

        }
    }
}