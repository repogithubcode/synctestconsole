using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class JsonData
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public object DataItem { get; set; }

        public JsonData()
        {
            Success = false;
            ErrorMessage = "";
            DataItem = null;
        }

        public JsonData(bool success, string errorMessage, object dataItem)
        {
            Success = success;
            ErrorMessage = errorMessage;
            DataItem = dataItem;
        }
    }
}