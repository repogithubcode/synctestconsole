using ProEstimator.Business.ProAdvisor;
using ProEstimator.DataRepositories.ProAdvisor;
using ProEstimatorData;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ProEstimator.Business.Test
{
    public class ProAdvisorProfileServiceTests
    {
        private IProAdvisorProfileService _profileService;

        public ProAdvisorProfileServiceTests()
        {
            _profileService = new ProAdvisorProfileService(
                new ProAdvisorProfileRepository(),
                new ProAdvisorPresetRepository(new ProAdvisorPresetShellRepository()),
                new ProAdvisorPresetShellRepository()
            ); 
        }

        [Fact]
        public void GetAllProfilesForLogin_ShouldReturnProfilesList()
        {
            // Arrange
            int loginID = 99115;
            bool showDeleted = false;

            // Act
            var profiles = _profileService.GetAllProfilesForLogin(loginID, showDeleted);

            // Assert
            Assert.NotNull(profiles);
            Assert.NotEmpty(profiles);
        }

        [Fact]
        public void GetProfile_ShouldReturnProfile()
        {
            // Arrange
            int profileID = 2;

            // Act
            var profile = _profileService.GetProfile(profileID);

            // Assert
            Assert.NotNull(profile);
        }

        [Fact]
        public void GetDefaultProfile_ShouldReturnDefaultProfile()
        {
            // Arrange
            int loginID = 99115;

            // Act
            var defaultProfile = _profileService.GetDefaultProfile(loginID);

            // Assert
            Assert.NotNull(defaultProfile);
        }

        [Fact]
        public void CopyProfile_ShouldCopyProfileAndReturnNewProfileId()
        {
            // Arrange
            int profileID = 2;
            int loginID = 99115;
            int activeLoginID = 0;

            // Act
            FunctionResultInt newProfileId = _profileService.CopyProfile(profileID, loginID, activeLoginID);

            // Assert
            Assert.True(newProfileId.Success);
            Assert.True(newProfileId.Value > 0);

            // Delete the new profile
            if (newProfileId.Value > 0)
            {
                DBAccess db = new DBAccess();
                db.ExecuteNonQuery("AddOnPresetProfile_PermanentDelete", new SqlParameter("ID", newProfileId.Value));
            }
        }

        [Fact]
        public void SetDefaultProfile_ShouldSetDefaultProfile()
        {
            // Arrange
            int loginID = 99115;
            int profileID = 2;

            // Act
            var result = _profileService.SetDefaultProfile(loginID, profileID);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void GetPresetsByFilters_ShouldReturnPresetsList()
        {
            // Arrange
            int siteUserID = 26577;     // Ezra's test account
            int profileID = 2;
            bool hasProAdvisorContract = true;
            string name = "";
            string operationType = "All";
            string laborType = "All";

            // Act
            var presets = _profileService.GetPresetsByFilters(siteUserID, profileID, hasProAdvisorContract, name, operationType, laborType);

            // Assert
            Assert.NotNull(presets);
            Assert.NotEmpty(presets);
        }

    }
}
