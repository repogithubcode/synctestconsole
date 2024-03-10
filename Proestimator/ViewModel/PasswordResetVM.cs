using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class PasswordResetVM
    {
        public bool GoodLink { get; set; }
        public string UserName { get; set; }
        public string NewPassword1 { get; set; }
        public string NewPassword2 { get; set; }
        public string Code { get; set; }
        public string ErrorMessage { get; set; }

        public PasswordResetVM()
        {

        }
    }
}