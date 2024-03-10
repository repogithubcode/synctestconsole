using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.SendEstimate.EMSExport
{
    public class SentEmsVM
    {
        public bool Success { get; set; }
        public int MessageQueueID { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Link { get; set; }

        public SentEmsVM()
        {
            Success = false;
            MessageQueueID = 0;
            Status = "";
            ErrorMessage = "";
            Link = "";
        }

    }
}