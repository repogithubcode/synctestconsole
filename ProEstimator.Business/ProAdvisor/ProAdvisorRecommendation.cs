using ProEstimatorData.DataModel.ProAdvisor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.ProAdvisor
{
    public class ProAdvisorRecommendation
    {
        public int ID { get; set; }
        public bool AutoSelect { get; set; }
        public string Name { get; set; }
        public string Charge { get; set; }

        public ProAdvisorRecommendation()
        {

        }

        public ProAdvisorRecommendation(ProAdvisorPreset preset)
        {
            ID = preset.ID;
            AutoSelect = preset.AutoSelect;
            Name = preset.PresetShell.Name;
        }
    }
}
