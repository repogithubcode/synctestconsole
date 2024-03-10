
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.OptOut.Commands;
using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.OptOut
{
    public class OptOutService : IOptOutService
    {
        
        /// <summary>
        /// Returns True if the passed LoginID has opted out of the passed OptOutType and optional DetailID.
        /// </summary>
        public bool HasOptedOut(OptOutType optOutType, int loginID, int detailID = 0)
        {
            // Get the most recent OptOut record
            List<OptOutLog> optOuts = OptOutLog.Get(loginID, optOutType, detailID);
            OptOutLog lastOptOut = optOuts.OrderByDescending(o => o.CreateStamp).FirstOrDefault();

            if (lastOptOut != null)
            {
                return !lastOptOut.IsDeleted;
            }

            return false;
        }

        /// <summary>
        /// Record that the passed LoginID has opted out of the passed OptOutType and optional DetailID.
        /// </summary>
        public void OptOut(OptOutType optOutType, int loginID, int detailID = 0)
        {
            OptOutLog.Insert(loginID, optOutType, detailID);
        }

        /// <summary>
        /// Get the OptOut history for LoginID and OptOutType and optional DetailID.
        /// </summary>
        public List<ProEstimatorData.DataModel.OptOutLog> GetHistory(OptOutType optOutType, int loginID, int detailID = 0)
        {
            return OptOutLog.Get(loginID, optOutType, detailID);
        }

        /// <summary>
        /// Encrypt the details of what to opt out of to be included in an email link.
        /// </summary>
        public string GetOptOutLink(OptOutType optOutType, int loginID, int detailID = 0)
        {
            string together = loginID + "::" + (int)optOutType + "::" + detailID;
            return Encryptor.Encrypt(together);
        }

        /// <summary>
        /// Processes an OptOut request.  Pass an encrypted string made by GetOptOutLink()
        /// </summary>
        public FunctionResult ProcessOptOutLink(string link)
        {
            ProcessOptOutLinkCommand command = new ProcessOptOutLinkCommand(this, link);
            if (command.Execute())
            {
                return new FunctionResult(true, command.SuccessMessage);
            }
            else
            {
                return new FunctionResult(command.ErrorMessage);
            }
        }
    }
}
