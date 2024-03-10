using ProEstimator.Business.Panels.LinkRules;
using ProEstimator.Business.Panels;
using ProEstimator.DataRepositories.Panels.LinkRules;
using ProEstimator.DataRepositories.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Test.Panels
{
    public class PanelServiceTests
    {
        private IPanelRepository _panelRepository;
        private ILinkRuleRepository _linkRuleRepository;
        private ILinkRuleService _linkRuleService;
        private ILinkRuleLineRepository _linkRuleLineRepository;

        private IPanelService _panelService;

        public PanelServiceTests()
        {
            // Initialize your repositories and service with actual implementations
            _panelRepository = new PanelRepository();
            _linkRuleRepository = new LinkRuleRepository();
            _linkRuleLineRepository = new LinkRuleLineRepository();
            LinkRulePresetLinkRepository presetLinkRepository = new LinkRulePresetLinkRepository();

            _linkRuleService = new LinkRuleService(_linkRuleRepository, _linkRuleLineRepository, presetLinkRepository);
            _panelService = new PanelService(_panelRepository, _linkRuleRepository, _linkRuleService);
        }

        [Fact]
        public void GetMainPanels_ShouldReturnListOfPanels()
        {
            // Act
            var result = _panelService.GetMainPanels();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }

        [Fact]
        public void GetPanel_ShouldReturnPanelById()
        {
            // Arrange
            int panelIdToRetrieve = 1; // Fender

            // Act
            var result = _panelService.GetPanel(panelIdToRetrieve);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(panelIdToRetrieve, result.ID);
            Assert.Equal("Fender", result.PanelName);
        }

        [Fact]
        public void GetForSection_ShouldReturnListOfPanels()
        {
            // Arrange
            string sectionDescription = "Front Fender"; 

            // Act
            var result = _panelService.GetForSection(sectionDescription);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void SavePanelDetails_ShouldSaveAndUpdatePanel()
        {
            // Arrange
            int panelIdToUpdate = 1; // Replace with an existing panel ID
            string newPanelName = "NewPanelName"; // Replace with the new panel name
                                                  // Initialize other parameters as needed

            Panel originalPanel = _panelService.GetPanel(panelIdToUpdate);
            string originalPanelName = originalPanel.PanelName;

            // Act
            var result = _panelService.SavePanelDetails(0, panelIdToUpdate, newPanelName, originalPanel.SortOrder, originalPanel.Symmetry, originalPanel.SectionLinkRuleID, originalPanel.PrimaryPanelLinkRuleID, "");

            try
            {
                // Assert
                Assert.True(result.Success);

                Panel changedPanel = _panelService.GetPanel(panelIdToUpdate);
                Assert.True(changedPanel.PanelName == newPanelName);
            }
            finally
            {
                _panelService.SavePanelDetails(0, panelIdToUpdate, originalPanelName, originalPanel.SortOrder, originalPanel.Symmetry, originalPanel.SectionLinkRuleID, originalPanel.PrimaryPanelLinkRuleID, "");
            }
        }

        [Fact]
        public void IsPrimaryPanelMatch_ShouldReturnTrueOrFalse()
        {
            // Arrange
            int panelId = 1; 
            string partDescription = "Fender Panel";
            Panel panel = _panelService.GetPanel(panelId);

            // Act
            var result = _panelService.IsPrimaryPanelMatch(panel, partDescription);

            // Assert
            Assert.True(result);
        }
    }
}
