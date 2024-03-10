using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.LinkRules
{
    public class LinkRulePresetLink : ProEstEntity, IIDSetter
    {
        public int ID { get; private set; }
        int IIDSetter.ID
        {
            set { ID = value; }
        }

        public int RuleID { get; set; }
        public int PresetID { get; set; }
        public string AddAction { get; set; }

    }
}
