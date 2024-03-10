using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.DataRepositories.Panels.LinkRules
{
    public class LinkRulePresetLinkRepository : ILinkRulePresetLinkRepository
    {
        public FunctionResult Save(LinkRulePresetLink linkRulePresetLink, int activeLoginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", linkRulePresetLink.ID));
            parameters.Add(new SqlParameter("LinkRuleID", linkRulePresetLink.RuleID));
            parameters.Add(new SqlParameter("PresetID", linkRulePresetLink.PresetID));
            parameters.Add(new SqlParameter("AddAction", linkRulePresetLink.AddAction));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("LinkRulePresetLink_Save", parameters);

            if (result.Success)
            {
                ((IIDSetter)linkRulePresetLink).ID = result.Value;
                ChangeLogManager.LogChange(activeLoginID, "LinkRulePresetLink", linkRulePresetLink.ID, 0, parameters, linkRulePresetLink.RowAsLoaded);
            }

            return (FunctionResult)result;
        }

        public FunctionResult Delete(int id)
        {
            DBAccess db = new DBAccess();
            return db.ExecuteNonQuery("LinkRulePresetLink_Delete", new SqlParameter("ID", id));
        }

        public List<LinkRulePresetLink> GetForRule(int ruleID, string addAction = "")
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LinkRuleID", ruleID));

            if (!string.IsNullOrEmpty(addAction))
            {
                parameters.Add(new SqlParameter("AddAction", addAction));
            }

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("LinkRulePresetLink_GetForRule", parameters);

            List<LinkRulePresetLink> presets = new List<LinkRulePresetLink>();

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                presets.Add(Initialize(row));
            }

            return presets;
        }

        private LinkRulePresetLink Initialize(DataRow row)
        {
            LinkRulePresetLink presetLink = new LinkRulePresetLink();

            ((IIDSetter)presetLink).ID = InputHelper.GetInteger(row["ID"].ToString());
            presetLink.RuleID = InputHelper.GetInteger(row["LinkRuleID"].ToString());
            presetLink.PresetID = InputHelper.GetInteger(row["PresetID"].ToString());
            presetLink.AddAction = InputHelper.GetString(row["AddAction"].ToString());

            presetLink.RowAsLoaded = row;

            return presetLink;
        }
    }

}
