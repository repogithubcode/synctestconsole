using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.ProAdvisor
{
    public class ProAdvisorPreset : ProEstEntity, IIDSetter
    {
        public int ID { get; private set; }
        int IIDSetter.ID
        {
            set { ID = value; }
        }

        public int ProfileID { get; set; }
        public ProAdvisorPresetShell PresetShell { get; set; }
        public double Labor { get; set; }
        public double Refinish { get; set; }
        public double Charge { get; set; }
        public string OtherTypeOverride { get; set; }
        public double OtherCharge { get; set; }
        public bool Active { get; set; }
        public bool AutoSelect { get; set; }

        public ProAdvisorPreset()
        {
            Labor = 0;
            Refinish = 0;
            Charge = 0;
            OtherTypeOverride = "";
            OtherCharge = 0;
        }
    }
}
