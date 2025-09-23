using FlowManager.Application.Interfaces;
using FlowManager.Application.Services;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs.Requests.Step;
using FlowManager.Shared.DTOs.Responses.Step;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FlowManager.Tests.Services
{
    [TestClass]
    public class StepServiceTests
    {
        private Mock<IStepRepository> _mockStepRepository;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IFlowRepository> _mockFlowRepository;
        private Mock<ITeamRepository> _mockTeamRepository;
        private Mock<IRoleRepository> _mockRoleRepository;
        private StepService _stepService;

        [TestInitialize]
        public void Setup()
        {
            _mockStepRepository = new Mock<IStepRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockFlowRepository = new Mock<IFlowRepository>();
            _mockTeamRepository = new Mock<ITeamRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();

            _stepService = new StepService(
                _mockStepRepository.Object,
                _mockUserRepository.Object,
                _mockFlowRepository.Object,
                _mockTeamRepository.Object,
                _mockRoleRepository.Object);
        }

        [TestMethod]
        public async Task GetStepAsync_WithValidId_ShouldReturnStep()
        {
            // Arrange
            var stepId = Guid.NewGuid();
            var expectedStep = new Step
            {
                Id = stepId,
                Name = "Test Step",
                Users = new List<User>(),
                FlowSteps = new List<FlowStep>()
            };

            _mockStepRepository
                .Setup(x => x.GetStepByIdAsync(stepId, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedStep);

            // Act
            var result = await _stepService.GetStepAsync(stepId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedStep.Id, result.StepId);
            Assert.AreEqual(expectedStep.Name, result.StepName);
        }

        [TestMethod]
        public async Task GetStepAsync_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            _mockStepRepository
                .Setup(x => x.GetStepByIdAsync(invalidId, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync((Step?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntryNotFoundException>(
                () => _stepService.GetStepAsync(invalidId));
        }

        [TestMethod]
        public async Task PostStepAsync_WithValidData_ShouldCreateStep()
        {
            // Arrange
            var request = new PostStepRequestDto
            {
                Name = "New Step"
            };

            var createdStep = new Step
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };

            _mockStepRepository
                .Setup(x => x.PostStepAsync(It.IsAny<Step>()))
                .ReturnsAsync(createdStep);

            // Act
            var result = await _stepService.PostStepAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(request.Name, result.StepName);
        }

        [TestMethod]
        public async Task DeleteStepAsync_WithValidId_ShouldDeleteStep()
        {
            // Arrange
            var stepId = Guid.NewGuid();
            var stepToDelete = new Step
            {
                Id = stepId,
                Name = "Step to Delete",
                Users = new List<User>()
            };

            _mockStepRepository
                .Setup(x => x.GetStepByIdAsync(stepId, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(stepToDelete);

            _mockStepRepository
                .Setup(x => x.DeleteStepAsync(stepToDelete))
                .ReturnsAsync(stepToDelete);

            // Act
            var result = await _stepService.DeleteStepAsync(stepId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(stepId, result.StepId);
            Assert.AreEqual(stepToDelete.Name, result.StepName);
        }

        [TestMethod]
        public async Task PatchStepAsync_WithValidData_ShouldUpdateStep()
        {
            // Arrange
            var stepId = Guid.NewGuid();
            var request = new PatchStepRequestDto
            {
                Name = "Updated Step"
            };

            var stepToUpdate = new Step
            {
                Id = stepId,
                Name = "Old Step Name",
                Users = new List<User>()
            };

            _mockStepRepository
                .Setup(x => x.GetStepByIdAsync(stepId, true, true, false, false))
                .ReturnsAsync(stepToUpdate);

            _mockStepRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _stepService.PatchStepAsync(stepId, request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(stepId, result.StepId);
            Assert.AreEqual(request.Name, result.StepName);
            Assert.AreEqual(request.Name, stepToUpdate.Name);
        }
    }
}