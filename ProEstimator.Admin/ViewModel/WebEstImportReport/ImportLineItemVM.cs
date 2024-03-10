using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;
using ProEstimatorData;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.WebEstImportReport
{
    public class ImportLineItemVM
    {
        public int LoginId { get; set; }
        public string LoginMoved { get; set; }
        public string CompanyName { get; set; }
        public string MovedBy { get; set; }
        public string SalesRep { get; set; }
        public string SelfImport { get; set; }
        public string ConversionComplete { get; set; }
        public bool Trial { get; set; }
        public bool ActiveTrial { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpirationDate { get; set; }

        public ImportLineItemVM() { }

        public ImportLineItemVM(ImportLineItem importLineItem)
        {
            LoginId = InputHelper.GetInteger(importLineItem.LoginId.ToString());
            LoginMoved = InputHelper.GetString(importLineItem.LoginMoved.ToString());
            CompanyName = InputHelper.GetString(importLineItem.CompanyName.ToString());
            MovedBy = InputHelper.GetString(importLineItem.MovedBy.ToString());
            SalesRep = InputHelper.GetString(importLineItem.SalesRep.ToString());
            SelfImport = InputHelper.GetString(importLineItem.SelfImport.ToString());
            ConversionComplete = InputHelper.GetString(importLineItem.ConversionComplete.ToString());
            Trial = InputHelper.GetBoolean(importLineItem.Trial.ToString());
            ActiveTrial = InputHelper.GetBoolean(importLineItem.ActiveTrial.ToString());
            EffectiveDate = InputHelper.GetString(importLineItem.EffectiveDate.ToString());
            ExpirationDate = InputHelper.GetString(importLineItem.ExpirationDate.ToString());
        }
    }
}