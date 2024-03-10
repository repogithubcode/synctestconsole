using ProEstimatorData.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimator.Admin.ViewModel.Logins
{
    public class AutoPayChangeVM
    {

        public string OnOrOff { get; set; }
        public string Reason { get; set; }
        public DateTime TimeStamp { get; set; }

        public AutoPayChangeVM()
        {

        }

        public AutoPayChangeVM(MiscTracking miscTracking)
        {
            OnOrOff = miscTracking.Tag.Replace("AutoPay", "");
            Reason = miscTracking.OtherData;
            TimeStamp = miscTracking.TimeStamp;
        }

    }
}