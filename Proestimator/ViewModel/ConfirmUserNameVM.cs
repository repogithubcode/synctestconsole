using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class ConfirmUserNameVM
    {

        public int UserID { get; set; }
        public string Organization { get; set; }
        public string EmailAddress { get; set; }
        public string NewUserName { get; set; }

        public string ErrorMessage { get; set; }

    }
}