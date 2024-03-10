using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class WebEstDataImportVM
    {

        public string LoginID { get; set; }

        public string Message { get; set; }

        public WebEstDataImportVM()
        {
            LoginID = "";
            Message = "";
        }

    }
}