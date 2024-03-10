using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Model.Account
{
    public static class GetAutoPayHistoryService
    {

        public static List<MiscTracking> GetAutoPayHistory(int loginID)
        {
            List<MiscTracking> turnOns = MiscTracking.Get(loginID, 0, "AutoPayOn");
            List<MiscTracking> turnOffs = MiscTracking.Get(loginID, 0, "AutoPayOff");

            List<MiscTracking> returnList = new List<MiscTracking>();
            returnList.AddRange(turnOns);
            returnList.AddRange(turnOffs);

            return returnList.OrderByDescending(o => o.TimeStamp).ToList();
        }

    }
}
