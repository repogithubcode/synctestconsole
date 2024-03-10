using ProEstimator.Business.ProAdvisor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.PDR.Model
{
    public class AddAdditionalOperationFunctionResult
    {
        public int CurrentPartIndex { get; set; }
        public int NewID { get; set; }
        public bool RIAdded { get; set; }

        public List<ProAdvisorRecommendation> AddOnResults { get; set; } = new List<ProAdvisorRecommendation>();
    }
}
