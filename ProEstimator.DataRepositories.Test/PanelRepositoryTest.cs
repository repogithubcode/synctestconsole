using ProEstimator.DataRepositories.Panels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ProEstimator.DataRepositories.Test
{
    public class PanelRepositoryTest
    {
        private readonly IPanelRepository _panelRepository;
        private readonly int _testPanelID = 1;

        public PanelRepositoryTest()
        {
            _panelRepository = new PanelRepository();
        }

        [Fact]
        public void GetAllPanels_Should_Return_At_Least_One_Panel()
        {
            // Act
            var panels = _panelRepository.GetAllPanels();

            // Assert
            Assert.NotNull(panels);
            Assert.True(panels.Count > 1);
        }

        [Fact]
        public void GetByID_Should_Return_Panel_With_Specified_ID()
        {
            // Act
            var panel = _panelRepository.GetByID(_testPanelID);

            // Assert
            Assert.NotNull(panel);
            Assert.Equal(_testPanelID, panel.ID);
        }

        [Fact]
        public void Save_Should_Update_Panel_Name_Successfully()
        {
            // Load panel with ID 1
            var panel = _panelRepository.GetByID(_testPanelID);
            Assert.NotNull(panel);

            // Modify the PanelName
            panel.PanelName += "a";

            // Act
            var result = _panelRepository.Save(panel, 0);

            // Assert
            Assert.True(result.Success);

            // Clean up by removing the added "a" from the PanelName
            panel.PanelName = panel.PanelName.Substring(0, panel.PanelName.Length - 1);
            _panelRepository.Save(panel, 0);
        }
    }
}
