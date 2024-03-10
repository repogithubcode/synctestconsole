using System.Collections.Generic;
using System.Data.SqlClient;

using ProEstimator.DataRepositories.Panels.LinkRules;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.Business.Panels.LinkRules.Commands
{
    internal class GetSectionDetailsForEstimateCommand : CommandBase
    {
        private ILinkRuleLineRepository _linkRuleLineRepository;
        private int _estimateID;
        private Panel _panel;

        public SectionDetailsResult SectionDetailsResult { get; private set; }

        public GetSectionDetailsForEstimateCommand(ILinkRuleLineRepository linkRuleLineRepository, int estimateID, Panel panel)
        {
            _linkRuleLineRepository = linkRuleLineRepository;
            _estimateID = estimateID;
            _panel = panel;
        }

        public override bool Execute()
        {
            if (_estimateID <= 0)
            {
                return Error("Invalid Estimate ID");
            }

            if (_panel == null)
            {
                return Error("Invalid Panel ID");
            }

            GetSqlWhereCommand getSqlWhereCommand = new GetSqlWhereCommand(_linkRuleLineRepository, _panel.SectionLinkRuleID);
            getSqlWhereCommand.Execute();
            string searchText = getSqlWhereCommand.WhereClause;

            if (string.IsNullOrEmpty(searchText))
            {
                return Error("Link rule not set up.");
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AdminInfoID", _estimateID));
            parameters.Add(new SqlParameter("Search", searchText));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("LinkRuleManager_GetSubSectionsForEstimate", parameters);

            if (tableResult.Success)
            {
                SectionDetailsResult = new SectionDetailsResult(tableResult);
                return true;
            }
            else
            {
                return Error(tableResult.ErrorMessage);
            }
        }
    }
}
