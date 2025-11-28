using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Blazored.LocalStorage;
using EscolaBiblica.BlazorWASM.Services;
using EscolaBiblica.BlazorWASM.Models;

namespace EscolaBiblica.Tests.Services;

public class CacheServiceTests
{
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly Mock<ILogger<CacheService>> _mockLogger;
    private readonly CacheService _cacheService;

    public CacheServiceTests()
    {
        _mockLocalStorage = new Mock<ILocalStorageService>();
        _mockLogger = new Mock<ILogger<CacheService>>();
        _cacheService = new CacheService(_mockLocalStorage.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SetAsync_ShouldStoreDataWithExpiration_WhenValidDataProvided()
    {
        // Arrange
        var key = "test-key";
        var data = new { Name = "Test", Value = 123 };
        var expiration = TimeSpan.FromMinutes(30);

        _mockLocalStorage.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<object>(), default))
                        .Returns(ValueTask.CompletedTask);

        // Act
        await _cacheService.SetAsync(key, data, expiration);

        // Assert
        _mockLocalStorage.Verify(x => x.SetItemAsync(
            $"cache_{key}",
            It.IsAny<object>(),
            default), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnData_WhenValidKeyAndDataExists()
    {
        // Arrange
        var key = "test-key";
        var expectedData = new { Name = "Test", Value = 123 };
        var cacheItem = new
        {
            Data = expectedData,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        _mockLocalStorage.Setup(x => x.GetItemAsync<object>($"cache_{key}", default))
                        .ReturnsAsync(cacheItem);

        // Act
        var result = await _cacheService.GetAsync<object>(key);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDefault_WhenKeyDoesNotExist()
    {
        // Arrange
        var key = "non-existent-key";
        
        _mockLocalStorage.Setup(x => x.GetItemAsync<object>($"cache_{key}", default))
                        .ReturnsAsync((object?)null);

        // Act
        var result = await _cacheService.GetAsync<object>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveItem_WhenValidKeyProvided()
    {
        // Arrange
        var key = "test-key";
        
        _mockLocalStorage.Setup(x => x.RemoveItemAsync($"cache_{key}", default))
                        .Returns(ValueTask.CompletedTask);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _mockLocalStorage.Verify(x => x.RemoveItemAsync($"cache_{key}", default), Times.Once);
    }

    [Fact]
    public async Task ClearAsync_ShouldClearAllCache_WhenCalled()
    {
        // Arrange
        var keys = new[] { "cache_key1", "cache_key2", "other_key", "cache_key3" };
        
        _mockLocalStorage.Setup(x => x.KeysAsync(default))
                        .ReturnsAsync(keys);
        
        _mockLocalStorage.Setup(x => x.RemoveItemAsync(It.IsAny<string>(), default))
                        .Returns(ValueTask.CompletedTask);

        // Act
        await _cacheService.ClearAsync();

        // Assert
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("cache_key1", default), Times.Once);
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("cache_key2", default), Times.Once);
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("cache_key3", default), Times.Once);
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("other_key", default), Times.Never);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        var key = "existing-key";
        var cacheItem = new
        {
            Data = "test-data",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        _mockLocalStorage.Setup(x => x.GetItemAsync<object>($"cache_{key}", default))
                        .ReturnsAsync(cacheItem);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var key = "non-existing-key";
        
        _mockLocalStorage.Setup(x => x.GetItemAsync<object>($"cache_{key}", default))
                        .ReturnsAsync((object?)null);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetAsync_ShouldHandleInvalidKeys_Gracefully(string invalidKey)
    {
        // Act & Assert
        var act = async () => await _cacheService.GetAsync<object>(invalidKey);
        
        if (string.IsNullOrWhiteSpace(invalidKey))
        {
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }

    [Fact]
    public async Task SetAsync_ShouldHandleZeroExpiration_BySettingDefaultExpiration()
    {
        // Arrange
        var key = "test-key";
        var data = "test-data";
        var expiration = TimeSpan.Zero;

        _mockLocalStorage.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<object>(), default))
                        .Returns(ValueTask.CompletedTask);

        // Act
        await _cacheService.SetAsync(key, data, expiration);

        // Assert
        _mockLocalStorage.Verify(x => x.SetItemAsync(
            $"cache_{key}",
            It.IsAny<object>(),
            default), Times.Once);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldReturnCachedData_WhenDataExistsAndNotExpired()
    {
        // Arrange
        var key = "test-key";
        var cachedData = "cached-value";
        var cacheItem = new
        {
            Data = cachedData,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        _mockLocalStorage.Setup(x => x.GetItemAsync<object>($"cache_{key}", default))
                        .ReturnsAsync(cacheItem);

        // Act
        var result = await _cacheService.GetOrSetAsync(key, () => Task.FromResult("new-value"), TimeSpan.FromMinutes(60));

        // Assert
        result.Should().Be(cachedData);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldCallFactoryAndCacheResult_WhenDataDoesNotExist()
    {
        // Arrange
        var key = "test-key";
        var newValue = "factory-value";
        var factoryCalled = false;
        
        _mockLocalStorage.Setup(x => x.GetItemAsync<object>($"cache_{key}", default))
                        .ReturnsAsync((object?)null);
        
        _mockLocalStorage.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<object>(), default))
                        .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _cacheService.GetOrSetAsync(key, () =>
        {
            factoryCalled = true;
            return Task.FromResult(newValue);
        }, TimeSpan.FromMinutes(30));

        // Assert
        result.Should().Be(newValue);
        factoryCalled.Should().BeTrue();
        _mockLocalStorage.Verify(x => x.SetItemAsync(
            $"cache_{key}",
            It.IsAny<object>(),
            default), Times.Once);
    }
}