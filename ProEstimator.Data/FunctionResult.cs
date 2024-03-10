using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData
{
    public class FunctionResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public FunctionResult()
        {
            Success = true;
            ErrorMessage = "";
        }

        public FunctionResult(string errorMessage)
        {
            Success = string.IsNullOrEmpty(errorMessage);
            ErrorMessage = errorMessage;
        }

        public FunctionResult(bool success, string message)
        {
            Success = success;
            ErrorMessage = message;
        }
    }    
}
