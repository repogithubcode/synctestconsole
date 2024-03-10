using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData.DataModel
{
    public class SaveResult
    {

        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }

        public SaveResult()
        {
            Success = true;
            ErrorMessage = "";
        }

        public SaveResult(string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Success = false;
                ErrorMessage = errorMessage;
            }
            else
            {
                Success = true;
                ErrorMessage = "";
            }
        }

        public SaveResult(FunctionResult dbAccessResult)
        {
            Success = dbAccessResult.Success;
            ErrorMessage = dbAccessResult.ErrorMessage;
        }
    }
}
