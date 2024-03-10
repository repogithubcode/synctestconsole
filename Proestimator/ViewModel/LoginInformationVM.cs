using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.Models;

namespace Proestimator.ViewModel
{
    public class LoginInformationVM
    {
        public int LoginID { get; set; }
        public string LoginFirstname {get;set;}
        public string LoginLastName { get; set; }
        public string LoginEmail { get; set; }
        public string LoginTitle { get; set; }
        public string LoginWorkNumber { get; set; }
        public string LoginFaxNumber { get; set; }

        public int EstimatorID { get; set; }
        public string EstimatorFirstName { get; set; }
        public string EstimatorLastName { get; set; }

        public List<EstimatorVM> Estimators { get; set; }

        public int? orderNo { get; set; }
    }
}