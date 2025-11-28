using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Blazored.LocalStorage;
using EscolaBiblica.BlazorWASM.Services;

namespace EscolaBiblica.Tests.Services;

public class SimplifiedCacheServiceTests
{
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly Mock<ILogger<CacheService>> _mockLogger;
    private readonly CacheService _cacheService;

    public SimplifiedCacheServiceTests()
    {
        _mockLocalStorage = new Mock<ILocalStorageService>();
        _mockLogger = new Mock<ILogger<CacheService>>();
        _cacheService = new CacheService(_mockLocalStorage.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SetAsync_ShouldCompleteWithoutError_WhenValidDataProvided()
    {
        // Arrange
        var key = "test-key";
        var data = "test-data";
        var expiration = TimeSpan.FromMinutes(30);

        _mockLocalStorage.Setup(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                        .Returns(ValueTask.CompletedTask);

        // Act & Assert
        await _cacheService.SetAsync(key, data, expiration);
        
        // If we get here without exception, the test passes
        Assert.True(true);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var key = "non-existent-key";
        
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync($"cache_{key}", default))
                        .ReturnsAsync((string?)null);

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ShouldCompleteWithoutError_WhenValidKeyProvided()
    {
        // Arrange
        var key = "test-key";

        _mockLocalStorage.Setup(x => x.RemoveItemAsync($"cache_{key}", default))
                        .Returns(ValueTask.CompletedTask);

        // Act & Assert
        await _cacheService.RemoveAsync(key);
        
        // If we get here without exception, the test passes
        Assert.True(true);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var key = "non-existent-key";
        
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync($"cache_{key}", default))
                        .ReturnsAsync((string?)null);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ClearAsync_ShouldCompleteWithoutError_WhenCalled()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.ClearAsync(default))
                        .Returns(ValueTask.CompletedTask);

        // Act & Assert
        await _cacheService.ClearAsync();
        
        // If we get here without exception, the test passes
        Assert.True(true);
    }
}