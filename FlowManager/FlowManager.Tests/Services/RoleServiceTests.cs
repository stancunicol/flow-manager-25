using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Services;
using FlowManager.Shared.DTOs.Responses.Role;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FlowManager.Tests.Services
{
    [TestClass]
    public class RoleServiceTests
    {
        private Mock<IRoleRepository> _mockRoleRepository;
        private RoleService _roleService;

        [TestInitialize]
        public void Setup()
        {
            _mockRoleRepository = new Mock<IRoleRepository>();
            _roleService = new RoleService(_mockRoleRepository.Object);
        }

        [TestMethod]
        public async Task GetAllRolesAsync_WithExistingRoles_ShouldReturnRoles()
        {
            // Arrange
            var roles = new List<Role>
            {
                new Role { Id = Guid.NewGuid(), Name = "Admin", NormalizedName = "ADMIN" },
                new Role { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER" },
                new Role { Id = Guid.NewGuid(), Name = "Moderator", NormalizedName = "MODERATOR" }
            };

            _mockRoleRepository
                .Setup(x => x.GetAllRolesAsync())
                .ReturnsAsync(roles);

            // Act
            var result = await _roleService.GetAllRolesAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Admin", result[0].Name);
        }

        [TestMethod]
        public async Task GetAllRolesAsync_WithNoRoles_ShouldThrowException()
        {
            // Arrange
            _mockRoleRepository
                .Setup(x => x.GetAllRolesAsync())
                .ReturnsAsync(new List<Role>());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntryNotFoundException>(
                () => _roleService.GetAllRolesAsync());
        }

        [TestMethod]
        public async Task GetAllRolesAsync_WithNullRoles_ShouldThrowException()
        {
            // Arrange
            _mockRoleRepository
                .Setup(x => x.GetAllRolesAsync())
                .ReturnsAsync((List<Role>?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntryNotFoundException>(
                () => _roleService.GetAllRolesAsync());
        }

        [TestMethod]
        public async Task GetRoleByIdAsync_WithValidId_ShouldReturnRole()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var expectedRole = new Role
            {
                Id = roleId,
                Name = "Admin",
                NormalizedName = "ADMIN"
            };

            _mockRoleRepository
                .Setup(x => x.GetRoleByIdAsync(roleId))
                .ReturnsAsync(expectedRole);

            // Act
            var result = await _roleService.GetRoleByIdAsync(roleId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedRole.Id, result.Id);
            Assert.AreEqual(expectedRole.Name, result.Name);
        }

        [TestMethod]
        public async Task GetRoleByIdAsync_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            _mockRoleRepository
                .Setup(x => x.GetRoleByIdAsync(invalidId))
                .ReturnsAsync((Role?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntryNotFoundException>(
                () => _roleService.GetRoleByIdAsync(invalidId));
        }
    }
}