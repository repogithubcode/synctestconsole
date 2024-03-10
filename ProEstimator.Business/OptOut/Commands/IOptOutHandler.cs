using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.OptOut.Commands
{
    internal interface IOptOutHandler
    {
        OptOutType OptOutType { get; }
        FunctionResult Process(IOptOutService optOutService, int loginID, int detailID);
    }
}
