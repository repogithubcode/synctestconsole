using ProEstimator.DataRepositories.ProAdvisor;
using ProEstimatorData.DataModel.ProAdvisor;
using ProEstimatorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ProEstimator.DataRepositories.Test
{
    public class ProAdvisorPresetRepositoryTests
    {
        private IProAdvisorPresetRepository repository = new ProAdvisorPresetRepository(new ProAdvisorPresetShellRepository());

        public ProAdvisorPresetRepositoryTests()
        {
            // Make sure there is a preset with ID = 1
            DBAccess db = new DBAccess();

            string query = @"SET IDENTITY_INSERT [dbo].[AddOnPreset] ON;
INSERT INTO [dbo].[AddOnPreset] ([ID], [ProfileID], [PresetShellID], [Labor], [Refinish], [Charge], [OtherTypeOverride], [OtherCharge], [Active], [AutoSelect])
SELECT 1, 0, 0, 0.0, 0.0, 0.0, '', 0.0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[AddOnPreset] WHERE [ID] = 1);
SET IDENTITY_INSERT [dbo].[AddOnPreset] OFF;";

            db.ExecuteSql(query);
        }

        [Fact]
        public void Save_ShouldSavePreset_WhenValidData()
        {
            // Arrange
            int presetId = 1;
            var preset = repository.Get(presetId);

            // Ensure that the preset with ID 1 exists for the test
            Assert.NotNull(preset);

            // Store the original property value for restoration
            var originalPropertyValue = preset.Labor;

            // Act - Modify the property
            preset.Labor = 123.45; // Modify the property value

            // Save the modified preset
            var saveResult = repository.Save(preset, 1);

            // Assert - Check if the save operation was successful
            Assert.True(saveResult.Success);

            // Act - Reset the property to its original value
            preset.Labor = originalPropertyValue;

            // Save the preset with the original value
            var restoreResult = repository.Save(preset, 1);

            // Assert - Check if the restore operation was successful
            Assert.True(restoreResult.Success);
        }

        [Fact]
        public void Get_ShouldRetrievePreset_WhenValidID()
        {
            // Arrange
            int presetID = 1;

            // Act
            ProAdvisorPreset retrievedPreset = repository.Get(presetID);

            // Assert
            Assert.NotNull(retrievedPreset);
            Assert.Equal(presetID, retrievedPreset.ID);
        }

        [Fact]
        public void GetForRateProfile_ShouldRetrievePresets_WhenValidProfileID()
        {
            // Arrange
            int profileID = 1;

            // Act
            List<ProAdvisorPreset> retrievedPresets = repository.GetForRateProfile(profileID);

            // Assert
            Assert.NotNull(retrievedPresets);
            Assert.NotEmpty(retrievedPresets);
        }

        [Fact]
        public void GetForRuleAndProfile_ShouldRetrievePresets_WhenValidParameters()
        {
            // Arrange
            int estimateID = 11879581; 
            int ruleID = 94;    
            int profileID = 1; 
            string action = "Replace";

            // Act
            List<ProAdvisorPreset> retrievedPresets = repository.GetForRuleAndProfile(estimateID, ruleID, profileID, action);

            // Assert
            Assert.NotNull(retrievedPresets);
            Assert.NotEmpty(retrievedPresets);
        }
    }
}
