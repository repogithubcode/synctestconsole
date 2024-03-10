using ProEstimator.Business.PDR.Model;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.DataModel;
using System.Collections.Generic;

namespace ProEstimator.Business.PDR
{
    public interface IPDRAdditionalOperationsService
    {
        SectionsForPDRPanelFunctionResult GetAdditionalOperations(int estimateID, int pdrPanelID);
        List<SectionPartInfo> GetPartsForPDRAdditionalOperations(List<SectionPartInfo> sectionParts, bool isLeftSide, bool isRightSide);
    }
}
