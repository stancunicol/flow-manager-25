using FlowManager.Application.IServices;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Application.Services;
using FlowManager.Shared.DTOs.Requests.Team;
using FlowManager.Shared.DTOs.Responses.Team;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FlowManager.Tests.Services
{
    [TestClass]
    public class TeamServiceTests
    {
        private Mock<ITeamRepository> _mockTeamRepository;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IRoleRepository> _mockRoleRepository;
        private TeamService _teamService;

        [TestInitialize]
        public void Setup()
        {
            _mockTeamRepository = new Mock<ITeamRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _teamService = new TeamService(_mockTeamRepository.Object, _mockUserRepository.Object, _mockRoleRepository.Object);
        }

        [TestMethod]
        public async Task GetTeamByIdAsync_WithValidId_ShouldReturnTeam()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var expectedTeam = new Team
            {
                Id = teamId,
                Name = "Test Team",
                Users = new List<UserTeam>()
            };

            _mockTeamRepository
                .Setup(x => x.GetTeamByIdAsync(teamId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedTeam);

            // Act
            var result = await _teamService.GetTeamByIdAsync(teamId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedTeam.Id, result.Id);
            Assert.AreEqual(expectedTeam.Name, result.Name);
        }

        [TestMethod]
        public async Task GetTeamByIdAsync_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            _mockTeamRepository
                .Setup(x => x.GetTeamByIdAsync(invalidId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync((Team?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntryNotFoundException>(
                () => _teamService.GetTeamByIdAsync(invalidId));
        }
    }
}