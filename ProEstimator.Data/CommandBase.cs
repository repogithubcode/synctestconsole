using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData
{
    public class CommandBase
    {

        public string ErrorMessage { get; protected set; }

        /// <summary>
        /// Sets the ErrorMessage and returns False.  This is done often and this allows 1 line of code instead of 2.
        /// </summary>
        public bool Error(string errorMessage)
        {
            ErrorMessage = errorMessage;
            return false;
        }

        public virtual bool Execute()
        {
            return false;
        }

    }
}
