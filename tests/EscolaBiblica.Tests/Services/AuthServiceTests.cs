using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EscolaBiblica.BlazorWASM.Services;

namespace EscolaBiblica.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _mockLocalStorage = new Mock<ILocalStorageService>();
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _mockHttpClient.Object,
            _mockLocalStorage.Object,
            _mockAuthStateProvider.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTrue_WhenAdminCredentialsProvided()
    {
        // Arrange
        var username = "admin";
        var password = "admin";
        
        _mockLocalStorage.Setup(x => x.SetItemAsync("authToken", It.IsAny<string>(), default))
                        .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _authService.LoginAsync(username, password);

        // Assert
        result.Should().BeTrue();
        _mockLocalStorage.Verify(x => x.SetItemAsync("authToken", It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFalse_WhenInvalidCredentialsProvided()
    {
        // Arrange
        var username = "invalid";
        var password = "invalid";

        // Act
        var result = await _authService.LoginAsync(username, password);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutAsync_ShouldRemoveTokens_WhenCalled()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.RemoveItemAsync("authToken", default))
                        .Returns(ValueTask.CompletedTask);
        _mockLocalStorage.Setup(x => x.RemoveItemAsync("refreshToken", default))
                        .Returns(ValueTask.CompletedTask);

        // Act
        await _authService.LogoutAsync();

        // Assert
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("authToken", default), Times.Once);
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("refreshToken", default), Times.Once);
    }

    [Fact]
    public async Task IsAuthenticatedAsync_ShouldReturnFalse_WhenNoTokenExists()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsync<string>("authToken", default))
                        .ReturnsAsync((string?)null);

        // Act
        var result = await _authService.IsAuthenticatedAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAuthenticatedAsync_ShouldReturnTrue_WhenValidTokenExists()
    {
        // Arrange
        var validToken = GenerateTestToken();
        _mockLocalStorage.Setup(x => x.GetItemAsync<string>("authToken", default))
                        .ReturnsAsync(validToken);

        // Act
        var result = await _authService.IsAuthenticatedAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnToken_WhenTokenExists()
    {
        // Arrange
        var expectedToken = "test-token";
        _mockLocalStorage.Setup(x => x.GetItemAsync<string>("authToken", default))
                        .ReturnsAsync(expectedToken);

        // Act
        var result = await _authService.GetTokenAsync();

        // Assert
        result.Should().Be(expectedToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("admin")]
    [InlineData("user@test.com")]
    [InlineData("testuser")]
    public async Task LoginAsync_ShouldHandleVariousUsernameFormats(string username)
    {
        // Arrange
        var password = username == "admin" ? "admin" : "wrongpassword";

        // Act
        var result = await _authService.LoginAsync(username, password);

        // Assert
        if (username == "admin" && password == "admin")
        {
            result.Should().BeTrue();
        }
        else
        {
            result.Should().BeFalse();
        }
    }

    [Fact]
    public async Task LoginAsync_ShouldGenerateValidAdminToken_WhenAdminCredentials()
    {
        // Arrange
        var username = "admin";
        var password = "admin";
        string capturedToken = string.Empty;
        
        _mockLocalStorage.Setup(x => x.SetItemAsync("authToken", It.IsAny<string>(), default))
                        .Callback<string, string, CancellationToken>((key, value, ct) => capturedToken = value)
                        .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _authService.LoginAsync(username, password);

        // Assert
        result.Should().BeTrue();
        capturedToken.Should().NotBeEmpty();
        capturedToken.Should().EndWith(".");
        capturedToken.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public async Task IsInRoleAsync_ShouldReturnCorrectRole_WhenAuthenticatedUserExists()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("role", "Admin"),
            new Claim("name", "Test User")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(principal);

        _mockAuthStateProvider.Setup(x => x.GetAuthenticationStateAsync())
                             .ReturnsAsync(authState);

        // Act
        var result = await _authService.IsInRoleAsync("Admin");

        // Assert
        result.Should().BeTrue();
    }

    private string GenerateTestToken()
    {
        // Generate a simple test token with future expiration
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"));
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{{\"sub\":\"test\",\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}}"));
        return $"{header}.{payload}.";
    }
}