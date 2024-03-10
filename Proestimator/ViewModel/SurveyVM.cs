using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class SurveyVM
    {
        public int LoginID { get; set; }
        public string SelectedValue { get; set; }
        public string Comments { get; set; }
        public string ErrorMessage { get; set; }
    }
}