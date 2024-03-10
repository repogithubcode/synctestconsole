using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class LoginVM
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string ErrorMessage { get; set; }
        public string Redirect { get; set; }
        public bool OtherLogins { get; set; }

        public bool NoOfLoginsExceeded { get; set; }

        public string EmailAddress { get; set; }
    }

    public class LoginOldVM
    {
        public string UserName { get; set; }
        public string Organization { get; set; }
        public string Password { get; set; }

        public string NewUserName { get; set; }
        public string NewPassword { get; set; }
        public string NewPassword2 { get; set; }

        public string ErrorMessage { get; set; }
        
    }
}