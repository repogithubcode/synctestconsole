using ProEstimator.DataRepositories.ProAdvisor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ProEstimator.DataRepositories.Test
{
    public class ProAdvisorPresetShellRepositoryTests
    {
        private IProAdvisorPresetShellRepository _repository;

        public ProAdvisorPresetShellRepositoryTests()
        {
            _repository = new ProAdvisorPresetShellRepository();
        }

        [Fact]
        public void Get_PresetShellWithId1_ShouldNotBeNull()
        {
            // Arrange
            int presetShellId = 1;

            // Act
            var presetShell = _repository.Get(presetShellId);

            // Assert
            Assert.NotNull(presetShell);
        }

        [Fact]
        public void Save_ModifyPresetShellName_ShouldChangeAndRestoreName()
        {
            // Arrange
            int presetShellId = 1;
            var presetShell = _repository.Get(presetShellId);

            // Ensure that the preset shell with ID 1 exists for the test
            Assert.NotNull(presetShell);

            // Store the original name for restoration
            var originalName = presetShell.Name;

            // Act - Modify the name
            presetShell.Name = "ModifiedName"; // Modify the name

            // Save the modified preset shell
            var saveResult = _repository.Save(presetShell, 1);

            // Assert - Check if the save operation was successful
            Assert.True(saveResult.Success);

            // Act - Load the preset shell again
            var modifiedPresetShell = _repository.Get(presetShellId);

            // Assert - Confirm the name change
            Assert.Equal("ModifiedName", modifiedPresetShell.Name);

            // Act - Reset the name to its original value
            presetShell.Name = originalName;

            // Save the preset shell with the original name
            var restoreResult = _repository.Save(presetShell, 1);

            // Assert - Check if the restore operation was successful
            Assert.True(restoreResult.Success);
        }

        [Fact]
        public void GetAll_ShouldReturnList()
        {
            // Act
            var presetShells = _repository.GetAll();

            // Assert
            Assert.NotNull(presetShells);
            Assert.NotEmpty(presetShells);
        }
    }
}
