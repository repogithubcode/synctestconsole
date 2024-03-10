using ProEstimatorData.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.DataRepositories.Parts
{
    public interface IPartsService
    {
        List<SectionPartInfo> GetPartsForSection(int estimateID, int sectionKey, bool allYears);
        List<SectionPartInfo> GetPartsForSection(int estimateID, int header, int section, bool allYears);
    }
}
