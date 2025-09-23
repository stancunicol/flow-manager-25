using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Services;
using FlowManager.Shared.DTOs.Requests.User;
using FlowManager.Shared.DTOs.Responses.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FlowManager.Tests.Services
{
    [TestClass]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IRoleRepository> _mockRoleRepository;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IPasswordHasher<User>> _mockPasswordHasher;
        private Mock<ITeamRepository> _mockTeamRepository;
        private Mock<IStepRepository> _mockStepRepository;
        private UserService _userService;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _mockPasswordHasher = new Mock<IPasswordHasher<User>>();
            _mockTeamRepository = new Mock<ITeamRepository>();
            _mockStepRepository = new Mock<IStepRepository>();

            _userService = new UserService(
                _mockUserRepository.Object,
                _mockRoleRepository.Object,
                _mockEmailService.Object,
                _mockPasswordHasher.Object,
                _mockTeamRepository.Object,
                _mockStepRepository.Object);
        }

        [TestMethod]
        public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var stepId = Guid.NewGuid();
            var expectedUser = new User
            {
                Id = userId,
                Name = "Test User",
                Email = "test@example.com",
                StepId = stepId,
                Step = new Step { Id = stepId, Name = "Test Step" },
                Roles = new List<UserRole>(),
                Teams = new List<UserTeam>()
            };

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUser.Id, result.Id);
            Assert.AreEqual(expectedUser.Name, result.Name);
            Assert.AreEqual(expectedUser.Email, result.Email);
        }

        [TestMethod]
        public async Task GetUserByIdAsync_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(invalidId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntryNotFoundException>(
                () => _userService.GetUserByIdAsync(invalidId));
        }

        [TestMethod]
        public async Task AddUserAsync_WithValidData_ShouldCreateUser()
        {
            // Arrange
            var stepId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var basicRoleId = Guid.NewGuid();
            var request = new PostUserRequestDto
            {
                Username = "newuser",
                Name = "New User",
                Email = "newuser@example.com",
                PhoneNumber = "123456789",
                Roles = new List<Guid> { roleId },
                StepId = stepId
            };

            var role = new Role { Id = roleId, Name = "User" };
            var basicRole = new Role { Id = basicRoleId, Name = "Basic", NormalizedName = "BASIC" };

            // Mock all the checks that AddUserAsync does
            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            _mockRoleRepository
                .Setup(x => x.GetRoleByIdAsync(roleId))
                .ReturnsAsync(role);

            _mockRoleRepository
                .Setup(x => x.GetRoleByRolenameAsync("Basic"))
                .ReturnsAsync(basicRole);

            _mockEmailService
                .Setup(x => x.SendWelcomeEmailAsync(request.Email, request.Name))
                .Returns(Task.CompletedTask);

            _mockUserRepository
                .Setup(x => x.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new User
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Email = request.Email,
                    UserName = request.Username,
                    Roles = new List<UserRole>(),
                    Teams = new List<UserTeam>()
                });

            // Act
            var result = await _userService.AddUserAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(request.Name, result.Name);
            Assert.AreEqual(request.Email, result.Email);
        }

        [TestMethod]
        public async Task DeleteUserAsync_WithValidId_ShouldMarkAsDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userToDelete = new User
            {
                Id = userId,
                Name = "User to Delete",
                Email = "delete@example.com",
                DeletedAt = null
            };

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(userToDelete);

            _mockUserRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userId, result.Id);
            Assert.IsNotNull(userToDelete.DeletedAt);
        }
    }
}