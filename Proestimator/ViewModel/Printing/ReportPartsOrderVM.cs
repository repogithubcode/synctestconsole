using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Mvc;

namespace Proestimator.ViewModel.Printing
{
    public class ReportPartsOrderVM
    {

        public int AdminInfoID { get; set; }
        public string UserContentPath { get { return ConfigurationManager.AppSettings.Get("UserContentPath").ToString().Replace(@"\", "/"); } }
        public string PartTypeFilter { get; set; }
        public int SupplementVersion { get; set; }
        public string RepairOrderNumber { get; set; }
        public string FedTaxID { get; set; }
        public string ReportHeader { get; set; }
        public List<SelectListItem>  ReportHeaderListItems { get; set; }

    }
}