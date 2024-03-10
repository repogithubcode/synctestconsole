using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Proestimator.ViewModel.Printing
{
    public class LetterCustomerVM
    {
        public int AdminInfoID { get; set; }
        public string JobNumber { get; set; }
        public string DateShort { get; set; }
        public string DateLong { get; set; }
        public string CompanyName { get; set; }
        public double MilesIn { get; set; }
        public double MilesOut { get; set; }
        public double GrandTotal { get; set; }
        public string UserContentPath { get { return ConfigurationManager.AppSettings.Get("UserContentPath").ToString().Replace(@"\", "/"); } }
    }
}