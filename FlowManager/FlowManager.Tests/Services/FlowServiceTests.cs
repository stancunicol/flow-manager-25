using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Services;
using FlowManager.Shared.DTOs.Requests.Flow;
using FlowManager.Shared.DTOs.Responses.Flow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FlowManager.Tests.Services
{
    [TestClass]
    public class FlowServiceTests
    {
        private Mock<IFlowRepository> _mockFlowRepository;
        private Mock<IFormTemplateRepository> _mockFormTemplateRepository;
        private Mock<IStepRepository> _mockStepRepository;
        private Mock<ITeamRepository> _mockTeamRepository;
        private Mock<IRoleRepository> _mockRoleRepository;
        private FlowService _flowService;

        [TestInitialize]
        public void Setup()
        {
            _mockFlowRepository = new Mock<IFlowRepository>();
            _mockFormTemplateRepository = new Mock<IFormTemplateRepository>();
            _mockStepRepository = new Mock<IStepRepository>();
            _mockTeamRepository = new Mock<ITeamRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();

            _flowService = new FlowService(
                _mockFlowRepository.Object,
                _mockFormTemplateRepository.Object,
                _mockStepRepository.Object,
                _mockTeamRepository.Object,
                _mockRoleRepository.Object);
        }

        [TestMethod]
        public async Task GetFlowByIdAsync_WithValidId_ShouldReturnFlow()
        {
            // Arrange
            var flowId = Guid.NewGuid();
            var moderatorRoleId = Guid.NewGuid();
            var expectedFlow = new Flow
            {
                Id = flowId,
                Name = "Test Flow",
                FlowSteps = new List<FlowStep>()
            };

            var moderatorRole = new Role
            {
                Id = moderatorRoleId,
                Name = "Moderator",
                NormalizedName = "MODERATOR"
            };

            _mockRoleRepository
                .Setup(x => x.GetRoleByRolenameAsync("MODERATOR"))
                .ReturnsAsync(moderatorRole);

            _mockFlowRepository
                .Setup(x => x.GetFlowByIdIncludeStepsAsync(flowId, moderatorRoleId))
                .ReturnsAsync(expectedFlow);

            // Act
            var result = await _flowService.GetFlowByIdAsync(flowId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedFlow.Id, result.Id);
            Assert.AreEqual(expectedFlow.Name, result.Name);
        }

        [TestMethod]
        public async Task GetFlowByIdAsync_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            var moderatorRoleId = Guid.NewGuid();

            var moderatorRole = new Role
            {
                Id = moderatorRoleId,
                Name = "Moderator",
                NormalizedName = "MODERATOR"
            };

            _mockRoleRepository
                .Setup(x => x.GetRoleByRolenameAsync("MODERATOR"))
                .ReturnsAsync(moderatorRole);

            _mockFlowRepository
                .Setup(x => x.GetFlowByIdIncludeStepsAsync(invalidId, moderatorRoleId))
                .ReturnsAsync((Flow?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntryNotFoundException>(
                () => _flowService.GetFlowByIdAsync(invalidId));
        }

        [TestMethod]
        public async Task CreateFlowAsync_WithValidData_ShouldCreateFlow()
        {
            // Arrange
            var request = new PostFlowRequestDto
            {
                Name = "New Flow"
            };

            var createdFlow = new Flow
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };

            _mockFlowRepository
                .Setup(x => x.GetFlowByNameAsync(request.Name))
                .ReturnsAsync((Flow?)null);

            _mockFlowRepository
                .Setup(x => x.CreateFlowAsync(It.IsAny<Flow>()))
                .ReturnsAsync(createdFlow);

            // Act
            var result = await _flowService.CreateFlowAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(request.Name, result.Name);
        }

        [TestMethod]
        public async Task DeleteFlowAsync_WithValidId_ShouldMarkAsDeleted()
        {
            // Arrange
            var flowId = Guid.NewGuid();
            var flowToDelete = new Flow
            {
                Id = flowId,
                Name = "Flow to Delete",
                DeletedAt = null
            };

            _mockFlowRepository
                .Setup(x => x.GetFlowByIdAsync(flowId))
                .ReturnsAsync(flowToDelete);

            _mockFlowRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _flowService.DeleteFlowAsync(flowId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(flowId, result.Id);
            Assert.IsNotNull(flowToDelete.DeletedAt);
        }
    }
}