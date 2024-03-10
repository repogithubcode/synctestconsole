using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Panels;
using ProEstimator.Business.Panels.LinkRules;
using ProEstimator.DataRepositories.Panels;
using ProEstimator.DataRepositories.Panels.LinkRules;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;
using Xunit;

namespace ProEstimator.Business.Test.Panels.LinkRules
{
    public class LinkRuleServiceTests : IDisposable
    {
        private readonly ILinkRuleRepository _ruleRepository;
        private readonly ILinkRuleLineRepository _lineRepository;
        private readonly ILinkRuleService _linkRuleService;
        private readonly IPanelRepository _panelRepository;

        public LinkRuleServiceTests()
        {
            _ruleRepository = new LinkRuleRepository();
            _lineRepository = new LinkRuleLineRepository();
            LinkRulePresetLinkRepository presetLinkRepository = new LinkRulePresetLinkRepository();
            _linkRuleService = new LinkRuleService(_ruleRepository, _lineRepository, presetLinkRepository);

            _panelRepository = new PanelRepository();
        }

        public void Dispose()
        {
            // Clean up resources, close database connections, etc., if needed.
            //DBAccess db = new DBAccess();
            //db.ExecuteSql("DELETE FROM LinkRule WHERE ID = 1");
        }

        // Test GetRule method
        [Fact]
        public void GetRule_ShouldReturnRule()
        {
            // Arrange
            int ruleId = 19;

            // Act
            var result = _linkRuleService.GetRule(ruleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ruleId, result.ID);
        }

        // Test CopyRule method
        [Fact]
        public void CopyRule_ShouldReturnCopiedRule()
        {
            // Arrange
            int ruleId = 19;

            // Act
            var originalRule = _linkRuleService.GetRule(ruleId);
            var copiedRule = _linkRuleService.CopyRule(originalRule);

            try
            {
                // Assert
                Assert.NotNull(copiedRule);
                Assert.NotEqual(originalRule.ID, copiedRule.ID);
                Assert.Equal(originalRule.RuleType, copiedRule.RuleType);
                Assert.True(copiedRule.ID > 0);
            }
            finally
            {
                // Clean up
                DBAccess db = new DBAccess();
                db.ExecuteSql("DELETE FROM LinkRule WHERE ID = " + copiedRule.ID);
            }
            
        }

        // Test LineMatchCheck method
        [Fact]
        public void LineMatchCheck_ShouldReturnTrue()
        {
            // Arrange
            int lineId = 28;
            string input = "rear door assembly";

            // Act
            var line = _lineRepository.Get(lineId);
            var result = _linkRuleService.LineMatchCheck(line, input);

            // Assert
            Assert.True(result);
        }

        // Test RuleMatchCheck method
        [Fact]
        public void RuleMatchCheck_ShouldReturnTrue()
        {
            // Arrange
            int ruleId = 19;
            string input = "rear door shell";

            // Act
            var rule = _ruleRepository.Get(ruleId);
            var result = _linkRuleService.RuleMatchCheck(rule, input);

            // Assert
            Assert.True(result);
        }

        // Test GetRuleSummaryText method
        [Fact]
        public void GetRuleSummaryText_ShouldReturnSummaryText()
        {
            // Arrange
            int ruleId = 11;

            // Act
            var rule = _ruleRepository.Get(ruleId);
            var summaryText = _linkRuleService.GetRuleSummaryText(rule);

            // Assert
            Assert.False(string.IsNullOrEmpty(summaryText));
            // Add more assertions as needed.
        }

        //// Test GetSectionDetailsForEstimate method
        //[Fact]
        //public void GetSectionDetailsForEstimate_ShouldReturnSectionDetailsResult()
        //{
        //    // Arrange
        //    int estimateId = 19;
        //    Panel panel = new Panel(); // Provide panel details if required.

        //    // Act
        //    var result = _linkRuleService.GetSectionDetailsForEstimate(estimateId, panel);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.True(result.Success);
        //    // Add more assertions as needed.
        //}

        // Test GetMatchDetailsForRule method
        [Fact]
        public void GetMatchDetailsForRule_ShouldReturnMatchDetails()
        {
            // Arrange
            Panel panel = _panelRepository.GetByID(1);
            LinkRuleType linkRuleType = LinkRuleType.PrimaryPanel;

            // Act
            var result = _linkRuleService.GetMatchDetailsForRule(panel, linkRuleType);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.MatchCountsTotal > 0);
            // Add more assertions as needed.
        }

    }

}
