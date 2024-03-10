using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.DataRepositories.Panels.LinkRules
{
    public class LinkRuleLineRepository : ILinkRuleLineRepository
    {
        public FunctionResult Save(LinkRuleLine linkRuleLine, int activeLoginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", linkRuleLine.ID));
            parameters.Add(new SqlParameter("LinkRuleID", linkRuleLine.LinkRuleID));
            parameters.Add(new SqlParameter("SortOrder", linkRuleLine.SortOrder));
            parameters.Add(new SqlParameter("MatchType", linkRuleLine.MatchType));
            parameters.Add(new SqlParameter("ChildRuleID", linkRuleLine.ChildRuleID));
            parameters.Add(new SqlParameter("MatchPiece", linkRuleLine.MatchPiece));
            parameters.Add(new SqlParameter("MatchText", linkRuleLine.MatchText));
            parameters.Add(new SqlParameter("Indented", linkRuleLine.Indented));
            parameters.Add(new SqlParameter("Disabled", linkRuleLine.Disabled));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("LinkRuleLine_Save", parameters);

            if (result.Success)
            {
                ((IIDSetter)linkRuleLine).ID = result.Value;
                ChangeLogManager.LogChange(activeLoginID, "LinkRuleLine", linkRuleLine.ID, 0, parameters, linkRuleLine.RowAsLoaded);
            }

            return (FunctionResult)result;
        }

        public LinkRuleLine Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("LinkRuleLine_Get", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return InstantiateLinkRuleLine(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public List<LinkRuleLine> GetForRule(int linkRuleID)
        {
            List<LinkRuleLine> results = new List<LinkRuleLine>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("LinkRuleLine_GetForRule", new SqlParameter("LinkRuleID", linkRuleID));

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                results.Add(InstantiateLinkRuleLine(row));
            }

            return results.OrderBy(o => o.SortOrder).ToList();
        }

        public List<LinkRuleLine> GetAll()
        {
            List<LinkRuleLine> results = new List<LinkRuleLine>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("LinkRuleLine_GetAll");

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                results.Add(InstantiateLinkRuleLine(row));
            }

            return results;
        }

        private LinkRuleLine InstantiateLinkRuleLine(DataRow row)
        {
            LinkRuleLine linkRuleLine = new LinkRuleLine();

            ((IIDSetter)linkRuleLine).ID = InputHelper.GetInteger(row["ID"].ToString());
            linkRuleLine.LinkRuleID = InputHelper.GetInteger(row["LinkRuleID"].ToString());
            linkRuleLine.SortOrder = InputHelper.GetInteger(row["SortOrder"].ToString());
            linkRuleLine.MatchType = (MatchType)Enum.Parse(typeof(MatchType), InputHelper.GetInteger(row["MatchType"].ToString(), 1).ToString());
            linkRuleLine.MatchPiece = (MatchPiece)Enum.Parse(typeof(MatchPiece), InputHelper.GetInteger(row["MatchPiece"].ToString(), 1).ToString());
            linkRuleLine.ChildRuleID = InputHelper.GetInteger(row["ChildRuleID"].ToString());
            linkRuleLine.MatchText = InputHelper.GetString(row["MatchText"].ToString());
            linkRuleLine.Indented = InputHelper.GetBoolean(row["Indented"].ToString());
            linkRuleLine.Disabled = InputHelper.GetBoolean(row["Disabled"].ToString());

            return linkRuleLine;
        }

        public FunctionResult Delete(int id)
        {
            DBAccess db = new DBAccess();
            FunctionResult deleteResult = db.ExecuteNonQuery("LinkRuleLine_Delete", new SqlParameter("ID", id));
            return deleteResult;
        }

        public LinkRuleLine Copy(LinkRuleLine lineToCopy, int newLinkRuleID)
        {
            LinkRuleLine newLine = new LinkRuleLine();
            newLine.LinkRuleID = newLinkRuleID;
            newLine.SortOrder = lineToCopy.SortOrder;
            newLine.MatchType = lineToCopy.MatchType;
            newLine.MatchPiece = lineToCopy.MatchPiece;
            newLine.ChildRuleID = lineToCopy.ChildRuleID;
            newLine.MatchText = lineToCopy.MatchText;
            newLine.Indented = lineToCopy.Indented;
            newLine.Disabled = lineToCopy.Disabled;

            newLine.Save();

            return newLine;
        }
    }

}
