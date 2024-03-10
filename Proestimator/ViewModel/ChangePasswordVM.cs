using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class ChangePasswordVM
    {
        public string OldPassword { get; set; }
        public string NewPassword1 { get; set; }
        public string NewPassword2 { get; set; }
        public string PasswordFailureText { get; set; }

        public int UserID { get; set; }
    }
}