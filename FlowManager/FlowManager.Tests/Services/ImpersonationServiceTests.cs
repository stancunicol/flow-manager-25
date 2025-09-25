using FlowManager.Application.Services;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using FlowManager.Shared.DTOs.Requests.Impersonation;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;

namespace FlowManager.Tests.Services
{
    [TestClass]
    public class ImpersonationServiceTests
    {
        private Mock<IImpersonationRepository> _mockRepository;
        private Mock<ILogger<ImpersonationService>> _mockLogger;
        private ImpersonationService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IImpersonationRepository>();
            _mockLogger = new Mock<ILogger<ImpersonationService>>();
            _service = new ImpersonationService(_mockRepository.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task StartImpersonationAsync_AdminUserNotFound_ReturnsNull()
        {
            // Arrange
            var request = new StartImpersonationRequestDto { UserId = Guid.NewGuid(), Reason = "Test" };
            var adminUserId = "admin123";

            _mockRepository.Setup(r => r.FindUserByIdAsync(adminUserId))
                          .ReturnsAsync((User?)null);

            // Act
            var result = await _service.StartImpersonationAsync(request, adminUserId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task StartImpersonationAsync_TargetUserNotFound_ReturnsNull()
        {
            // Arrange
            var request = new StartImpersonationRequestDto { UserId = Guid.NewGuid(), Reason = "Test" };
            var adminUserId = "admin123";
            var adminUser = new User { Id = Guid.NewGuid(), Name = "Admin User" };

            _mockRepository.Setup(r => r.FindUserByIdAsync(adminUserId))
                          .ReturnsAsync(adminUser);
            _mockRepository.Setup(r => r.FindUserByIdWithStepAsync(request.UserId))
                          .ReturnsAsync((User?)null);

            // Act
            var result = await _service.StartImpersonationAsync(request, adminUserId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task StartImpersonationAsync_TargetUserIsAdmin_ReturnsNull()
        {
            // Arrange
            var request = new StartImpersonationRequestDto { UserId = Guid.NewGuid(), Reason = "Test" };
            var adminUserId = "admin123";
            var adminUser = new User { Id = Guid.NewGuid(), Name = "Admin User" };
            var targetUser = new User { Id = request.UserId, Name = "Target User" };

            _mockRepository.Setup(r => r.FindUserByIdAsync(adminUserId))
                          .ReturnsAsync(adminUser);
            _mockRepository.Setup(r => r.FindUserByIdWithStepAsync(request.UserId))
                          .ReturnsAsync(targetUser);
            _mockRepository.Setup(r => r.GetUserRolesAsync(targetUser))
                          .ReturnsAsync(new List<string> { "Admin", "User" });

            // Act
            var result = await _service.StartImpersonationAsync(request, adminUserId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task StartImpersonationAsync_ValidRequest_ReturnsImpersonationResult()
        {
            // Arrange
            var request = new StartImpersonationRequestDto { UserId = Guid.NewGuid(), Reason = "Test impersonation" };
            var adminUserId = "admin123";
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin User",
                Email = "admin@test.com"
            };
            var targetUser = new User
            {
                Id = request.UserId,
                Name = "Target User",
                Email = "target@test.com"
            };
            var targetRoles = new List<string> { "User", "Employee" };

            _mockRepository.Setup(r => r.FindUserByIdAsync(adminUserId))
                          .ReturnsAsync(adminUser);
            _mockRepository.Setup(r => r.FindUserByIdWithStepAsync(request.UserId))
                          .ReturnsAsync(targetUser);
            _mockRepository.Setup(r => r.GetUserRolesAsync(targetUser))
                          .ReturnsAsync(targetRoles);

            // Act
            var result = await _service.StartImpersonationAsync(request, adminUserId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(targetUser.Id, result.Response.ImpersonatedUser.Id);
            Assert.AreEqual(targetUser.Name, result.Response.ImpersonatedUser.Name);
            Assert.AreEqual(targetUser.Email, result.Response.ImpersonatedUser.Email);
            Assert.AreEqual(targetRoles.Count, result.Response.ImpersonatedUser.Roles.Count);
            Assert.IsTrue(result.Claims.Any(c => c.Type == "IsImpersonating" && c.Value == "true"));
            Assert.IsTrue(result.Claims.Any(c => c.Type == "OriginalAdminId" && c.Value == adminUser.Id.ToString()));
            Assert.IsTrue(result.Claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == targetUser.Id.ToString()));
        }

        [TestMethod]
        public async Task EndImpersonationAsync_NotImpersonating_ReturnsNull()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user123"),
                new Claim(ClaimTypes.Name, "User Name")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _service.EndImpersonationAsync(principal);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task EndImpersonationAsync_MissingOriginalAdminId_ReturnsNull()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user123"),
                new Claim("IsImpersonating", "true")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _service.EndImpersonationAsync(principal);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task EndImpersonationAsync_OriginalAdminNotFound_ReturnsNull()
        {
            // Arrange
            var originalAdminId = "admin123";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user123"),
                new Claim("IsImpersonating", "true"),
                new Claim("OriginalAdminId", originalAdminId)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _mockRepository.Setup(r => r.FindUserByIdAsync(originalAdminId))
                          .ReturnsAsync((User?)null);

            // Act
            var result = await _service.EndImpersonationAsync(principal);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task EndImpersonationAsync_ValidRequest_ReturnsEndImpersonationResult()
        {
            // Arrange
            var originalAdminId = "admin123";
            var originalAdmin = new User
            {
                Id = Guid.NewGuid(),
                Name = "Original Admin",
                Email = "admin@test.com"
            };
            var adminRoles = new List<string> { "Admin" };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user123"),
                new Claim(ClaimTypes.Name, "Impersonated User"),
                new Claim("IsImpersonating", "true"),
                new Claim("OriginalAdminId", originalAdminId),
                new Claim("OriginalAdminName", "Original Admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _mockRepository.Setup(r => r.FindUserByIdAsync(originalAdminId))
                          .ReturnsAsync(originalAdmin);
            _mockRepository.Setup(r => r.GetUserRolesAsync(originalAdmin))
                          .ReturnsAsync(adminRoles);

            // Act
            var result = await _service.EndImpersonationAsync(principal);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(originalAdmin.Id, result.OriginalAdmin.Id);
            Assert.AreEqual(originalAdmin.Name, result.OriginalAdmin.Name);
            Assert.IsTrue(result.AdminClaims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == originalAdmin.Id.ToString()));
            Assert.IsTrue(result.AdminClaims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin"));
        }

        [TestMethod]
        public void GetImpersonationStatus_IsImpersonating_ReturnsTrue()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("IsImpersonating", "true")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = _service.GetImpersonationStatus(principal);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetImpersonationStatus_NotImpersonating_ReturnsFalse()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user123")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = _service.GetImpersonationStatus(principal);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetOriginalAdminName_HasClaim_ReturnsName()
        {
            // Arrange
            var adminName = "Original Admin";
            var claims = new List<Claim>
            {
                new Claim("OriginalAdminName", adminName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = _service.GetOriginalAdminName(principal);

            // Assert
            Assert.AreEqual(adminName, result);
        }

        [TestMethod]
        public void GetOriginalAdminName_NoClaim_ReturnsEmpty()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user123")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = _service.GetOriginalAdminName(principal);

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void GetCurrentUserName_HasClaim_ReturnsName()
        {
            // Arrange
            var userName = "Current User";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = _service.GetCurrentUserName(principal);

            // Assert
            Assert.AreEqual(userName, result);
        }

        [TestMethod]
        public void GetCurrentUserName_NoClaim_ReturnsEmpty()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user123")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = _service.GetCurrentUserName(principal);

            // Assert
            Assert.AreEqual("", result);
        }
    }
}