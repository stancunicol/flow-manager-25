using FlowManager.Client.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlowManager.Tests.Services
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private AuthService _authService;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://localhost:5001/")
            };
            _authService = new AuthService(_httpClient);
        }

        [TestMethod]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnTrue()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = await _authService.LoginAsync(email, password);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFalse()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrongpassword";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

            // Act
            var result = await _authService.LoginAsync(email, password);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task LogoutAsync_ShouldReturnTrue()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = await _authService.LogoutAsync();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task RegisterAsync_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var name = "Test User";
            var email = "test@example.com";
            var password = "password123";
            var role = "User";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

            // Act
            var result = await _authService.RegisterAsync(name, email, password, role);

            // Assert
            Assert.IsTrue(result);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _httpClient?.Dispose();
        }
    }
}