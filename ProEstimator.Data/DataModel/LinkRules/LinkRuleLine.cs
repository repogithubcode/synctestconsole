using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.LinkRules
{
    public class LinkRuleLine : ProEstEntity, IIDSetter
    {

        public int ID { get; private set; }
        int IIDSetter.ID
        {
            set { ID = value; }
        }

        public int LinkRuleID { get; set; }
        public int SortOrder { get; set; }
        public MatchType MatchType { get; set; }
        public MatchPiece MatchPiece { get; set; }
        public int ChildRuleID { get; set; }
        public string MatchText { get; set; }
        public bool Indented { get; set; }
        public bool Disabled { get; set; }

        public LinkRuleLine()
        {
            MatchType = MatchType.Include;
        }        
    }
}
