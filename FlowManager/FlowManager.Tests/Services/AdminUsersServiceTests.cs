using FlowManager.Application.Services;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FlowManager.Tests.Services
{
    [TestClass]
    public class AdminUsersServiceTests
    {
        private Mock<IAdminUsersRepository> _mockRepository;
        private AdminUsersService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IAdminUsersRepository>();
            _service = new AdminUsersService(_mockRepository.Object);
        }

        [TestMethod]
        public async Task GetUsersForImpersonationAsync_NoUsers_ReturnsEmptyList()
        {
            // Arrange
            var currentUserId = "admin123";
            var search = "";
            var page = 1;
            var pageSize = 50;

            _mockRepository.Setup(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize))
                          .ReturnsAsync(new List<User>());

            // Act
            var result = await _service.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetUsersForImpersonationAsync_WithNonAdminUsers_ReturnsUserProfiles()
        {
            // Arrange
            var currentUserId = "admin123";
            var search = "";
            var page = 1;
            var pageSize = 50;

            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Name = "User One",
                Email = "user1@test.com",
                UserName = "user1",
                Roles = new List<UserRole>
                {
                    new UserRole { Role = new Role { Name = "User" } },
                    new UserRole { Role = new Role { Name = "Employee" } }
                }
            };

            var user2 = new User
            {
                Id = Guid.NewGuid(),
                Name = "User Two",
                Email = "user2@test.com",
                UserName = "user2",
                Roles = new List<UserRole>
                {
                    new UserRole { Role = new Role { Name = "Manager" } }
                }
            };

            var users = new List<User> { user1, user2 };

            _mockRepository.Setup(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize))
                          .ReturnsAsync(users);

            // Act
            var result = await _service.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            var profile1 = result.First(p => p.Id == user1.Id);
            Assert.AreEqual(user1.Name, profile1.Name);
            Assert.AreEqual(user1.Email, profile1.Email);
            Assert.AreEqual(user1.UserName, profile1.UserName);
            Assert.AreEqual(2, profile1.Roles.Count);
            Assert.IsTrue(profile1.Roles.Contains("User"));
            Assert.IsTrue(profile1.Roles.Contains("Employee"));

            var profile2 = result.First(p => p.Id == user2.Id);
            Assert.AreEqual(user2.Name, profile2.Name);
            Assert.AreEqual(user2.Email, profile2.Email);
            Assert.AreEqual(user2.UserName, profile2.UserName);
            Assert.AreEqual(1, profile2.Roles.Count);
            Assert.IsTrue(profile2.Roles.Contains("Manager"));
        }

        [TestMethod]
        public async Task GetUsersForImpersonationAsync_WithAdminUsers_ExcludesAdmins()
        {
            // Arrange
            var currentUserId = "admin123";
            var search = "";
            var page = 1;
            var pageSize = 50;

            var regularUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Regular User",
                Email = "user@test.com",
                UserName = "user",
                Roles = new List<UserRole>
                {
                    new UserRole { Role = new Role { Name = "User" } }
                }
            };

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin User",
                Email = "admin@test.com",
                UserName = "admin",
                Roles = new List<UserRole>
                {
                    new UserRole { Role = new Role { Name = "Admin" } },
                    new UserRole { Role = new Role { Name = "User" } }
                }
            };

            var users = new List<User> { regularUser, adminUser };

            _mockRepository.Setup(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize))
                          .ReturnsAsync(users);

            // Act
            var result = await _service.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(regularUser.Id, result[0].Id);
            Assert.AreEqual(regularUser.Name, result[0].Name);
        }

        [TestMethod]
        public async Task GetUsersForImpersonationAsync_WithNullEmail_HandlesNullEmail()
        {
            // Arrange
            var currentUserId = "admin123";
            var search = "";
            var page = 1;
            var pageSize = 50;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "User With Null Email",
                Email = null,
                UserName = "user_null_email",
                Roles = new List<UserRole>
                {
                    new UserRole { Role = new Role { Name = "User" } }
                }
            };

            var users = new List<User> { user };

            _mockRepository.Setup(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize))
                          .ReturnsAsync(users);

            // Act
            var result = await _service.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(user.Id, result[0].Id);
            Assert.AreEqual(user.Name, result[0].Name);
            Assert.AreEqual("", result[0].Email);
            Assert.AreEqual(user.UserName, result[0].UserName);
        }

        [TestMethod]
        public async Task GetUsersForImpersonationAsync_WithSearchParameter_PassesToRepository()
        {
            // Arrange
            var currentUserId = "admin123";
            var search = "john";
            var page = 1;
            var pageSize = 50;

            _mockRepository.Setup(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize))
                          .ReturnsAsync(new List<User>());

            // Act
            await _service.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize);

            // Assert
            _mockRepository.Verify(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize), Times.Once);
        }

        [TestMethod]
        public async Task GetUsersForImpersonationAsync_WithPaginationParameters_PassesToRepository()
        {
            // Arrange
            var currentUserId = "admin123";
            var search = "";
            var page = 2;
            var pageSize = 25;

            _mockRepository.Setup(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize))
                          .ReturnsAsync(new List<User>());

            // Act
            await _service.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize);

            // Assert
            _mockRepository.Verify(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize), Times.Once);
        }

        [TestMethod]
        public async Task GetUsersForImpersonationAsync_WithNullCurrentUserId_PassesToRepository()
        {
            // Arrange
            string? currentUserId = null;
            var search = "";
            var page = 1;
            var pageSize = 50;

            _mockRepository.Setup(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize))
                          .ReturnsAsync(new List<User>());

            // Act
            await _service.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize);

            // Assert
            _mockRepository.Verify(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize), Times.Once);
        }

        [TestMethod]
        public async Task GetUsersForImpersonationAsync_WithNullSearch_PassesToRepository()
        {
            // Arrange
            var currentUserId = "admin123";
            string? search = null;
            var page = 1;
            var pageSize = 50;

            _mockRepository.Setup(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize))
                          .ReturnsAsync(new List<User>());

            // Act
            await _service.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize);

            // Assert
            _mockRepository.Verify(r => r.GetUsersForImpersonationAsync(currentUserId, search, page, pageSize), Times.Once);
        }
    }
}