using ProEstimator.DataRepositories.Panels.LinkRules;
using ProEstimatorData;
using ProEstimatorData.DataModel.LinkRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ProEstimator.Business.Panels.LinkRules.Commands
{
    internal class GetSqlWhereCommand : CommandBase
    {
        public string WhereClause { get; private set; }

        private ILinkRuleLineRepository _linkRuleLineRepository;
        private int _ruleID;

        public GetSqlWhereCommand(ILinkRuleLineRepository linkRuleLineRepository, int ruleID)
        {
            _linkRuleLineRepository = linkRuleLineRepository;
            _ruleID = ruleID;
        }

        public override bool Execute()
        {
            StringBuilder builder = new StringBuilder();

            List<LinkRuleLine> ruleLines = _linkRuleLineRepository.GetForRule(_ruleID);

            foreach (LinkRuleLine line in ruleLines.Where(o => !o.Disabled))
            {
                if (line.ChildRuleID > 0)
                {
                    if (builder.Length > 0)
                    {
                        if (line.Indented)
                        {
                            builder.Append(" OR ");
                        }
                        else
                        {
                            builder.Append(" AND ");
                        }
                    }

                    if (!line.Indented)
                    {
                        builder.Append("(");
                    }

                    builder.Append("(");

                    GetSqlWhereCommand childCommand = new GetSqlWhereCommand(_linkRuleLineRepository, line.ChildRuleID);
                    childCommand.Execute();
                    builder.Append(childCommand.WhereClause);
                    
                    builder.Append(")");
                }
                else
                {
                    string comparer = GetComparer(line);
                    if (!string.IsNullOrEmpty(comparer))
                    {
                        if (!line.Indented && builder.Length > 0)
                        {
                            builder.Append(")");
                        }

                        if (builder.Length > 0)
                        {
                            if (line.Indented)
                            {
                                builder.Append(" OR ");
                            }
                            else
                            {
                                builder.Append(" AND ");
                            }
                        }

                        if (!line.Indented)
                        {
                            builder.Append("(");
                        }

                        builder.Append(comparer);
                    }
                }
            }

            if (builder.Length > 0)
            {
                builder.Append(")");
            }

            WhereClause = builder.ToString();
            return true;
        }

        /// <summary>
        /// Returns text to include in a sql WHERE clause, such as "Category.Category LIKE '%fender%'"
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string GetComparer(LinkRuleLine line)
        {
            if (line.MatchPiece == MatchPiece.ChildRule)
            {
                return "";
            }

            if (string.IsNullOrEmpty(line.MatchText) && line.MatchPiece != MatchPiece.SubCategory)
            {
                return "";
            }

            StringBuilder builder = new StringBuilder();

            if (line.MatchPiece == MatchPiece.Category)
            {
                builder.Append("Category.Category ");
            }
            else if (line.MatchPiece == MatchPiece.SubCategory)
            {
                builder.Append("SubCategory.Subcategory ");
            }
            else if (line.MatchPiece == MatchPiece.Part)
            {
                builder.Append("dbo.ProcessPartDescrption(CASE WHEN Detail.Prtc_Description LIKE 'L %' OR Detail.Prtc_Description LIKE 'R %' THEN SUBSTRING(Detail.Prtc_Description, 3, LEN(Detail.Prtc_Description) - 2) ELSE Detail.Prtc_Description END) ");
            }

            string comparer = "";
            if (line.MatchType == MatchType.Include)
            {
                comparer = "LIKE '%" + line.MatchText + "%'";
            }
            else if (line.MatchType == MatchType.NotInclude)
            {
                comparer = "NOT LIKE '%" + line.MatchText + "%'";
            }
            else if (line.MatchType == MatchType.ExactMatch)
            {
                if (line.MatchPiece == MatchPiece.SubCategory && string.IsNullOrEmpty(line.MatchText))
                {
                    comparer = " = '' OR SubCategory.Subcategory = Category.Category";
                }
                else
                {
                    comparer = " = '" + line.MatchText + "'";
                }
            }
            builder.Append(comparer);

            return builder.ToString();
        }
    }
}
