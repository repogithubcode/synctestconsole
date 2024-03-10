using System.Collections.Generic;

namespace ProEstimator.DataRepositories.PartnerNetwork
{
    public interface IPartnerNetworkRepository
    {
        List<ProEstimatorData.DataModel.PartnerNetwork> GetAll();

        void InsertClick(int partnerID, int activeLoginID);
    }
}
