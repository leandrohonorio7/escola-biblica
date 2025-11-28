using Blazored.LocalStorage;
using System.Text.Json;

namespace EscolaBiblica.BlazorWASM.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task ClearAsync();
    Task<bool> ExistsAsync(string key);
}

public class CacheService : ICacheService
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<CacheService> _logger;
    private const string CachePrefix = "cache_";
    private const string ExpiryPrefix = "expiry_";

    public CacheService(ILocalStorageService localStorage, ILogger<CacheService> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cacheKey = GetCacheKey(key);
            var expiryKey = GetExpiryKey(key);

            // Verificar se a entrada expirou
            var expiryString = await _localStorage.GetItemAsStringAsync(expiryKey);
            if (!string.IsNullOrEmpty(expiryString))
            {
                if (DateTime.TryParse(expiryString, out var expiry) && expiry < DateTime.UtcNow)
                {
                    await RemoveAsync(key);
                    return default;
                }
            }

            var cachedData = await _localStorage.GetItemAsStringAsync(cacheKey);
            if (string.IsNullOrEmpty(cachedData))
                return default;

            return JsonSerializer.Deserialize<T>(cachedData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao recuperar item do cache: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var cacheKey = GetCacheKey(key);
            var expiryKey = GetExpiryKey(key);

            var serializedData = JsonSerializer.Serialize(value);
            await _localStorage.SetItemAsStringAsync(cacheKey, serializedData);

            if (expiry.HasValue)
            {
                var expiryTime = DateTime.UtcNow.Add(expiry.Value);
                await _localStorage.SetItemAsStringAsync(expiryKey, expiryTime.ToString("O"));
            }

            _logger.LogDebug("Item adicionado ao cache: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar item ao cache: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var cacheKey = GetCacheKey(key);
            var expiryKey = GetExpiryKey(key);

            await _localStorage.RemoveItemAsync(cacheKey);
            await _localStorage.RemoveItemAsync(expiryKey);

            _logger.LogDebug("Item removido do cache: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover item do cache: {Key}", key);
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _localStorage.ClearAsync();
            _logger.LogInformation("Cache limpo completamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar cache");
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var cacheKey = GetCacheKey(key);
            var cachedData = await _localStorage.GetItemAsStringAsync(cacheKey);
            return !string.IsNullOrEmpty(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência no cache: {Key}", key);
            return false;
        }
    }

    private static string GetCacheKey(string key) => $"{CachePrefix}{key}";
    private static string GetExpiryKey(string key) => $"{ExpiryPrefix}{key}";
}

// Extensões para facilitar o uso do cache
public static class CacheServiceExtensions
{
    public static async Task<T> GetOrSetAsync<T>(this ICacheService cache, string key, Func<Task<T>> getItem, TimeSpan? expiry = null)
    {
        var cachedItem = await cache.GetAsync<T>(key);
        if (cachedItem != null)
            return cachedItem;

        var item = await getItem();
        if (item != null)
            await cache.SetAsync(key, item, expiry);

        return item;
    }

    public static async Task<T> GetOrSetAsync<T>(this ICacheService cache, string key, Func<T> getItem, TimeSpan? expiry = null)
    {
        var cachedItem = await cache.GetAsync<T>(key);
        if (cachedItem != null)
            return cachedItem;

        var item = getItem();
        if (item != null)
            await cache.SetAsync(key, item, expiry);

        return item;
    }
}