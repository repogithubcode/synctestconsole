using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Proestimator.ViewModel.Printing
{
    public class LetterFinalVM
    {

        public int AdminInfoID { get; set; }
        public string DateString { get; set; }
        public float RepairOrderAmount { get; set; }
        public float SupplementAmount { get; set; }
        public float TotalAmount { get; set; }
        public float CurrentAmount { get; set; }
        public bool ShowEstimateNumber { get; set; }
        public bool ShowID { get; set; }
        public string UserContentPath { get { return ConfigurationManager.AppSettings.Get("UserContentPath").ToString().Replace(@"\", "/"); } }

    }
}