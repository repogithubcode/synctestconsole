using ProEstimatorData.DataModel.LinkRules;
using ProEstimatorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.PDR.Model
{
    public class SectionsForPDRPanelFunctionResult : FunctionResult
    {
        public PanelDetailsVM PanelDetails { get; set; }
        public List<PanelDetailsVM> AdjacentPanelDetails { get; set; } = new List<PanelDetailsVM>();
    }
}
