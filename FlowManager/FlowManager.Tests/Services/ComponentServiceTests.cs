using FlowManager.Application.IServices;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Application.Services;
using FlowManager.Shared.DTOs.Requests.Component;
using FlowManager.Shared.DTOs.Responses.Component;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FlowManager.Tests.Services
{
    [TestClass]
    public class ComponentServiceTests
    {
        private Mock<IComponentRepository> _mockComponentRepository;
        private ComponentService _componentService;

        [TestInitialize]
        public void Setup()
        {
            _mockComponentRepository = new Mock<IComponentRepository>();
            _componentService = new ComponentService(_mockComponentRepository.Object);
        }

        [TestMethod]
        public async Task GetComponentByIdAsync_WithValidId_ShouldReturnComponent()
        {
            // Arrange
            var componentId = Guid.NewGuid();
            var expectedComponent = new Component
            {
                Id = componentId,
                Type = "TextInput",
                Label = "Test Label",
                Required = false,
                Properties = new Dictionary<string, object>()
            };

            _mockComponentRepository
                .Setup(x => x.GetComponentByIdAsync(componentId))
                .ReturnsAsync(expectedComponent);

            // Act
            var result = await _componentService.GetComponentByIdAsync(componentId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedComponent.Id, result.Id);
            Assert.AreEqual(expectedComponent.Type, result.Type);
        }

        [TestMethod]
        public async Task GetComponentByIdAsync_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            _mockComponentRepository
                .Setup(x => x.GetComponentByIdAsync(invalidId))
                .ReturnsAsync((Component?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntryNotFoundException>(
                () => _componentService.GetComponentByIdAsync(invalidId));
        }
    }
}