using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;
using ProEstimatorData.DataModel.Profiles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.DataRepositories.Panels.LinkRules
{
    public class LinkRuleRepository : ILinkRuleRepository
    {
        public FunctionResult Save(LinkRule linkRule, int activeLoginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", linkRule.ID));
            parameters.Add(new SqlParameter("RuleType", (int)linkRule.RuleType));
            parameters.Add(new SqlParameter("Deleted", linkRule.Deleted));
            parameters.Add(new SqlParameter("Enabled", linkRule.Enabled));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("LinkRule_Save", parameters);

            if (result.Success)
            {
                ((IIDSetter)linkRule).ID = result.Value;
                ChangeLogManager.LogChange(activeLoginID, "LinkRule", linkRule.ID, 0, parameters, linkRule.RowAsLoaded);
            }

            return (FunctionResult)result;
        }

        private LinkRule Instantiate(DataRow row)
        {
            LinkRule linkRule = new LinkRule();

            ((IIDSetter)linkRule).ID = InputHelper.GetInteger(row["ID"].ToString());
            linkRule.RuleType = (LinkRuleType)InputHelper.GetInteger(row["RuleType"].ToString());
            linkRule.Deleted = InputHelper.GetBoolean(row["Deleted"].ToString());
            linkRule.Enabled = InputHelper.GetBoolean(row["Enabled"].ToString());

            linkRule.RowAsLoaded = row;

            return linkRule;
        }

        public LinkRule Get(int ruleID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("LinkRule_Get", new SqlParameter("ID", ruleID));

            if (tableResult.Success)
            {
                return Instantiate(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public List<LinkRule> GetAll()
        {
            List<LinkRule> linkRules = new List<LinkRule>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("LinkRule_GetAll");

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                linkRules.Add(Instantiate(row));
            }

            return linkRules;
        }
        
    }
}
