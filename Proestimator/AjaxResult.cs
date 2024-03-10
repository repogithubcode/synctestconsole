using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator
{
    /// <summary>
    /// A simple base class for returning data from an Ajax call.  Return an instance of this or a derived class from a Controller method with a 
    /// JsonResult return type that is called from jquery ajax on a view.
    /// </summary>
    public class AjaxResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }

        public AjaxResult()
        {
            Success = false;
            Message = "";
            ErrorMessage = "";
        }
    }

    public class AjaxPingImpersonationResult : AjaxResult
    {
        public bool IsImpersonated { get; set; } = false;
        public bool AdminIsImpersonating { get; set; } = false;
        public bool? AutoSaveTurnedOnTechSupport { get; set; } = true;
        public bool? AutoSaveTurnedOnSiteUser { get; set; } = true;
        public bool? ImpersonationToLogout { get; set; } = true;

        public AjaxPingImpersonationResult()
        {
            IsImpersonated = false;
            AdminIsImpersonating = false;
            AutoSaveTurnedOnTechSupport = false;
            AutoSaveTurnedOnSiteUser = false;
        }
    }
}