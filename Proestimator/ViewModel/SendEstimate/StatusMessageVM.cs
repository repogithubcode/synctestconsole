using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.SendEstimate
{
    public class StatusMessageVM
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Event { get; set; }
        public string Reason { get; set; }
        public int EmailID { get; set; }

        public StatusMessageVM()
        {

        }
    }
}