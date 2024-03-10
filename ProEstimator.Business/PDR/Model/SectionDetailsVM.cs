
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.Business.PDR.Model
{
    public class SectionDetailsVM
    {
        public string Name { get; set; }
        public int Section { get; set; }
        public int Header { get; set; }
        public int SectionKey { get; set; }

        public List<SectionPartInfo> Parts { get; set; } = new List<SectionPartInfo>();
    }
}
