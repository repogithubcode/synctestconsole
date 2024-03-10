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

namespace ProEstimator.Admin.ViewModel.Errors
{
    public class ErrorLogReportVM
    {
        public int Id { get; set; }
        public int LoginID { get; set; }
        public int AdminInfoID { get; set; }
        public string ErrorText { get; set; }
        public string TimeOccurred { get; set; }
        public string SessionVars { get; set; }
        public bool ErrorFixed { get; set; }
        public string FixNote { get; set; }
        public string App { get; set; }
        public SelectList ErrorTagDDL { get; set; }
        public string SelectedErrorTag { get; set; }

        public ErrorLogReportVM() { }

        public ErrorLogReportVM(ErrorLog errorLog)
        {
            Id = InputHelper.GetInteger(errorLog.Id.ToString());
            LoginID = InputHelper.GetInteger(errorLog.LoginID.ToString());
            AdminInfoID = InputHelper.GetInteger(errorLog.AdminInfoID.ToString());
            ErrorText = InputHelper.GetString(errorLog.ErrorText.ToString());
            TimeOccurred = InputHelper.GetString(errorLog.TimeOccurred.ToString());
            SessionVars = InputHelper.GetString(errorLog.SessionVars.ToString());
            ErrorFixed = InputHelper.GetBoolean(errorLog.ErrorFixed.ToString());

            if(!string.IsNullOrEmpty(errorLog.FixNote))
            {
                FixNote = InputHelper.GetString(errorLog.FixNote.ToString());
            }
            
            App = InputHelper.GetString(errorLog.App.ToString());
        }
    }
}