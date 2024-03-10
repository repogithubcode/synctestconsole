using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel.SendEstimate
{
    public class SentEstimateSummaryVM
    {
        public int ID { get; set; }
        public DateTime TimeStamp { get; set; }
        public string RecipientList { get; set; }
        public string Message { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }

        public SentEstimateSummaryVM()
        {

        }

        public SentEstimateSummaryVM(ReportEmail email)
        {
            ID = email.ID;
            TimeStamp = email.SentStamp;
            Message = email.Subject;
            HasError = GetHasError(email);
            ErrorMessage = email.Errors;

            RecipientList = (email.ToAddresses + " " + email.PhoneNumbers).Trim();
            if (RecipientList.Length > 50)
            {
                RecipientList = RecipientList.Substring(0, 50) + "...";
            }
        }

        private bool GetHasError(ReportEmail email)
        {
            if(!string.IsNullOrEmpty(email.Errors.Trim()))
            {
                return true;
            }
            List<SendGridInfo> infos = SendGridInfo.GetByEmailID(email.EmailID);
            foreach(SendGridInfo info in infos)
            {
                if (info.Event != "delivered")
                {
                    return true;
                }
            }
            return false;
        }

    }
}