using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

using ProEstimator.DataRepositories.Panels.LinkRules;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.Business.Panels.LinkRules.Commands
{
    internal class GetMatchDetailsForRuleCommand : CommandBase
    {
        private ILinkRuleLineRepository _linkRuleLineRepository;

        private Panel _panel;
        private LinkRuleType _linkRuleType;

        public MatchDetails MatchDetails { get; private set; }

        public GetMatchDetailsForRuleCommand(ILinkRuleLineRepository linkRuleLineRepository, Panel panel, LinkRuleType linkRuleType)
        {
            _linkRuleLineRepository = linkRuleLineRepository;
            _panel = panel;
            _linkRuleType = linkRuleType;

            MatchDetails = new MatchDetails();
        }

        public override bool Execute()
        {
            try
            {
                // Put together a sql LIKE string for the search text
                StringBuilder builder = new StringBuilder();

                // Always start with the section container where clause
                string sectionContainerFilters = GetSqlWhere(_panel.SectionLinkRuleID);
                if (!string.IsNullOrEmpty(sectionContainerFilters))
                {
                    builder.Append("(");
                    builder.Append(sectionContainerFilters);
                    builder.Append(")");
                }

                // Add the Primary Section where clause onto the base section container
                if (_linkRuleType == LinkRuleType.PrimarySection || _linkRuleType == LinkRuleType.PrimaryPanel)
                {
                    string primarySectionFilters = GetSqlWhere(_panel.PrimarySectionLinkRuleID);
                    if (!string.IsNullOrEmpty(primarySectionFilters))
                    {
                        builder.Append(" AND ");
                        builder.Append("(");
                        builder.Append(primarySectionFilters);
                        builder.Append(")");
                    }
                }

                // Add the Primary Panel if appropriate
                if (_linkRuleType == LinkRuleType.PrimaryPanel)
                {
                    string primaryPanelFilters = GetSqlWhere(_panel.PrimaryPanelLinkRuleID);
                    if (!string.IsNullOrEmpty(primaryPanelFilters))
                    {
                        builder.Append(" AND ");
                        builder.Append("(");
                        builder.Append(primaryPanelFilters);
                        builder.Append(")");
                    }
                }

                if (builder.Length > 0)
                {
                    // Get the data in a data set with 2 tables
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("Search", builder.ToString()));

                    string spName = "LinkRule_GetCategoryMatchDetailsSummary";
                    if (_linkRuleType == LinkRuleType.PrimaryPanel)
                    {
                        spName = "LinkRule_GetPartMatchDetailsSummary";
                    }

                    DBAccess db = new DBAccess();
                    DBAccessDataSetResult dataSet = db.ExecuteWithDataSet(spName, parameters);
                    MatchDetails = new MatchDetails(dataSet.DataSet);
                }
            }
            catch(Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "GetMatchDetailsForRuleCommand");
            }

            return true;
        }

        private string GetSqlWhere(int ruleID)
        {
            GetSqlWhereCommand command = new GetSqlWhereCommand(_linkRuleLineRepository, ruleID);
            command.Execute();
            return command.WhereClause;
        }

    }
}
