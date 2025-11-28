using System.Net.Http.Json;
using System.Text.Json;

namespace EscolaBiblica.BlazorWASM.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object data);
    Task<T?> PutAsync<T>(string endpoint, object data);
    Task DeleteAsync(string endpoint);
    Task<HttpResponseMessage> PostAsync(string endpoint, object data);
    Task<HttpResponseMessage> PutAsync(string endpoint, object data);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            _logger.LogInformation("GET request to {Endpoint}", endpoint);
            var response = await _httpClient.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }

            _logger.LogWarning("GET request failed: {StatusCode} - {Endpoint}", response.StatusCode, endpoint);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GET request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            _logger.LogInformation("POST request to {Endpoint}", endpoint);
            var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }

            _logger.LogWarning("POST request failed: {StatusCode} - {Endpoint}", response.StatusCode, endpoint);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            _logger.LogInformation("PUT request to {Endpoint}", endpoint);
            var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }

            _logger.LogWarning("PUT request failed: {StatusCode} - {Endpoint}", response.StatusCode, endpoint);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PUT request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task DeleteAsync(string endpoint)
    {
        try
        {
            _logger.LogInformation("DELETE request to {Endpoint}", endpoint);
            var response = await _httpClient.DeleteAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DELETE request failed: {StatusCode} - {Endpoint}", response.StatusCode, endpoint);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DELETE request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<HttpResponseMessage> PostAsync(string endpoint, object data)
    {
        _logger.LogInformation("POST request to {Endpoint}", endpoint);
        return await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
    }

    public async Task<HttpResponseMessage> PutAsync(string endpoint, object data)
    {
        _logger.LogInformation("PUT request to {Endpoint}", endpoint);
        return await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);
    }
}