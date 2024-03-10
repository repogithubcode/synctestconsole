using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimatorData.DataModel
{
    public class Panel : ProEstEntity, IIDSetter
    {

        public int ID { get; private set; }
        int IIDSetter.ID
        {
            set { ID = value; }
        }

        public string PanelName { get; set; }
	    public int SortOrder { get; set; }
        public bool Symmetry { get; set; }
        public int SectionLinkRuleID { get; set; }
        public int PrimarySectionLinkRuleID { get; set; }
        public int PrimaryPanelLinkRuleID { get; set; }
    }
}
