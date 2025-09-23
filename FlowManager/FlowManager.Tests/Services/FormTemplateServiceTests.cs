using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Services;
using FlowManager.Shared.DTOs.Requests.FormTemplate;
using FlowManager.Shared.DTOs.Responses.FormTemplate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FlowManager.Tests.Services
{
    [TestClass]
    public class FormTemplateServiceTests
    {
        private Mock<IFormTemplateRepository> _mockFormTemplateRepository;
        private Mock<IComponentRepository> _mockComponentRepository;
        private Mock<IFlowRepository> _mockFlowRepository;
        private FormTemplateService _formTemplateService;

        [TestInitialize]
        public void Setup()
        {
            _mockFormTemplateRepository = new Mock<IFormTemplateRepository>();
            _mockComponentRepository = new Mock<IComponentRepository>();
            _mockFlowRepository = new Mock<IFlowRepository>();
            _formTemplateService = new FormTemplateService(
                _mockFormTemplateRepository.Object,
                _mockComponentRepository.Object,
                _mockFlowRepository.Object);
        }

        [TestMethod]
        public async Task GetFormTemplateByIdAsync_WithValidId_ShouldReturnFormTemplate()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var expectedTemplate = new FormTemplate
            {
                Id = templateId,
                Name = "Test Template",
                Content = "Test Content"
            };

            _mockFormTemplateRepository
                .Setup(x => x.GetFormTemplateByIdAsync(templateId))
                .ReturnsAsync(expectedTemplate);

            // Act
            var result = await _formTemplateService.GetFormTemplateByIdAsync(templateId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedTemplate.Id, result.Id);
            Assert.AreEqual(expectedTemplate.Name, result.Name);
        }

        [TestMethod]
        public async Task GetFormTemplateByIdAsync_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            _mockFormTemplateRepository
                .Setup(x => x.GetFormTemplateByIdAsync(invalidId))
                .ReturnsAsync((FormTemplate?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntryNotFoundException>(
                () => _formTemplateService.GetFormTemplateByIdAsync(invalidId));
        }
    }
}