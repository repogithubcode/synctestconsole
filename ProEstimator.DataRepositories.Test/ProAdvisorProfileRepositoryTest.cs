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
    public class ProAdvisorProfileRepositoryTest
    {
        private IProAdvisorProfileRepository repository = new ProAdvisorProfileRepository();

        [Fact]
        public void GetProfile_ShouldReturnProfile_WhenValidProfileID()
        {
            // Arrange
            int profileID = 1;

            // Act
            ProAdvisorPresetProfile profile = repository.GetProfile(profileID);

            // Assert
            Assert.NotNull(profile);
            Assert.Equal(profileID, profile.ID);
        }

        [Fact]
        public void GetAllProfilesForAccount_ShouldReturnProfiles_WhenValidLoginID()
        {
            // Arrange
            int loginID = 56027; 
            bool showDeleted = false; 

            // Act
            List<ProAdvisorPresetProfile> profiles = repository.GetAllProfilesForAccount(loginID, showDeleted);

            // Assert
            Assert.NotNull(profiles);
            Assert.True(profiles.Count > 0);
        }

        [Fact]
        public void SaveProfile_ShouldReturnSuccess_WhenValidProfile()
        {
            // Arrange
            int activeLoginID = 1; 
            int profileID = 1; 

            // Load the original profile
            ProAdvisorPresetProfile originalProfile = repository.GetProfile(profileID);

            // Change the profile name
            string originalName = originalProfile.Name;
            string newName = "NewProfileName"; 
            originalProfile.Name = newName;

            // Act
            FunctionResult result = repository.SaveProfile(activeLoginID, originalProfile);

            // Assert
            Assert.True(result.Success);
            Assert.True(string.IsNullOrEmpty(result.ErrorMessage));

            // Reload the profile
            ProAdvisorPresetProfile updatedProfile = repository.GetProfile(profileID);

            // Check if the profile name has been updated
            Assert.Equal(newName, updatedProfile.Name);

            // Change the name back to the original name
            updatedProfile.Name = originalName;

            // Save the updated profile with the original name
            FunctionResult restoreResult = repository.SaveProfile(activeLoginID, updatedProfile);

            // Assert that the name has been restored successfully
            Assert.True(restoreResult.Success);
            Assert.True(string.IsNullOrEmpty(restoreResult.ErrorMessage));
        }
    }
}
