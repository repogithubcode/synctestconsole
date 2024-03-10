using ProEstimatorData;
using ProEstimatorData.DataModel;
using System.Collections.Generic;

namespace ProEstimator.Business.OptOut
{
    public interface IOptOutService
    {

        void OptOut(OptOutType optOutType, int loginID, int detailID = 0);
        bool HasOptedOut(OptOutType optOutType, int loginID, int detailID = 0);
        List<ProEstimatorData.DataModel.OptOutLog> GetHistory(OptOutType optOutType, int loginID, int detailID = 0);
        string GetOptOutLink(OptOutType optOutType, int loginID, int detailID = 0);
        FunctionResult ProcessOptOutLink(string link);

    }
}
