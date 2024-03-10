using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class SandboxVM
    {

        public string UserName { get; set; }
        public string Extension { get; set; }
        public string Password { get; set; }

        public string FromNumber { get; set; }
        public string ToNumber { get; set; }
        public string Body { get; set; }

        public string SalesRepID { get; set; }
        public string SignupToNumber { get; set; }
        public string LoginID { get; set; }

        public string ErrorMessage { get; set; }

    }
}