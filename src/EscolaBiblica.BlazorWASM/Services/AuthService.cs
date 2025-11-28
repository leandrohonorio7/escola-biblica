using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace EscolaBiblica.BlazorWASM.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string emailOrUsername, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    Task<ClaimsPrincipal> GetUserAsync();
    Task<bool> IsInRoleAsync(string role);
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<AuthService> _logger;
    private const string TokenKey = "authToken";
    private const string RefreshTokenKey = "refreshToken";

    public AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider,
        ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    public async Task<bool> LoginAsync(string emailOrUsername, string password)
    {
        try
        {
            // Verificar se é o usuário admin padrão
            if (emailOrUsername.ToLower() == "admin" && password == "admin")
            {
                var adminToken = GenerateAdminToken();
                await _localStorage.SetItemAsync(TokenKey, adminToken);
                
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", adminToken);
                
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(adminToken);
                
                _logger.LogInformation("Login de administrador realizado");
                return true;
            }

            var loginRequest = new { EmailOrUsername = emailOrUsername, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (tokenResponse?.AccessToken != null)
                {
                    await _localStorage.SetItemAsync(TokenKey, tokenResponse.AccessToken);
                    if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                    {
                        await _localStorage.SetItemAsync(RefreshTokenKey, tokenResponse.RefreshToken);
                    }

                    // Configurar o header Authorization para futuras requisições
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

                    // Notificar o AuthenticationStateProvider sobre a mudança
                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(tokenResponse.AccessToken);

                    _logger.LogInformation("Login realizado com sucesso para {EmailOrUsername}", emailOrUsername);
                    return true;
                }
            }

            _logger.LogWarning("Falha no login para {EmailOrUsername}: {StatusCode}", emailOrUsername, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o login para {EmailOrUsername}", emailOrUsername);
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(RefreshTokenKey);
            
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
            
            _logger.LogInformation("Logout realizado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o logout");
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return jwtToken.ValidTo > DateTime.UtcNow;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(TokenKey);
    }

    public async Task<ClaimsPrincipal> GetUserAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User;
    }

    public async Task<bool> IsInRoleAsync(string role)
    {
        var user = await GetUserAsync();
        return user.IsInRole(role);
    }

    private string GenerateAdminToken()
    {
        var claims = new List<Claim>
        {
            new("sub", "admin"),
            new("email", "admin@escolabiblica.com"),
            new("name", "Administrador"),
            new("role", "Admin"),
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new("exp", DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds().ToString())
        };

        // Criar um JWT simples (apenas para autenticação local)
        var header = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(new { alg = "none", typ = "JWT" }));
        var payload = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(claims.ToDictionary(c => c.Type, c => c.Value)));
        
        return $"{header}.{payload}.";
    }

    private class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
    }
}

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<CustomAuthStateProvider> _logger;
    private const string TokenKey = "authToken";

    public CustomAuthStateProvider(ILocalStorageService localStorage, ILogger<CustomAuthStateProvider> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>(TokenKey);
            
            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Verificar se é token customizado do admin (sem assinatura)
            if (token.EndsWith("."))
            {
                var parts = token.Split('.');
                if (parts.Length >= 2)
                {
                    var payload = parts[1];
                    var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                    var jsonBytes = Convert.FromBase64String(paddedPayload);
                    var claimsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes) ?? new Dictionary<string, object>();
                    
                    // Verificar expiração
                    if (claimsDict.TryGetValue("exp", out var expObj) && expObj != null)
                    {
                        if (long.TryParse(expObj.ToString(), out var exp))
                        {
                            var expiryTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                            if (expiryTime < DateTimeOffset.UtcNow)
                            {
                                await _localStorage.RemoveItemAsync(TokenKey);
                                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                            }
                        }
                    }
                    
                    var claims = claimsDict.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? "")).ToList();
                    var identity = new ClaimsIdentity(claims, "custom");
                    var user = new ClaimsPrincipal(identity);
                    
                    return new AuthenticationState(user);
                }
            }
            else
            {
                // Token JWT padrão
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    await _localStorage.RemoveItemAsync(TokenKey);
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var claims = jwtToken.Claims.ToList();
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estado de autenticação");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        try
        {
            ClaimsPrincipal user;
            
            // Verificar se é token customizado do admin
            if (token.EndsWith("."))
            {
                var parts = token.Split('.');
                if (parts.Length >= 2)
                {
                    var payload = parts[1];
                    var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                    var jsonBytes = Convert.FromBase64String(paddedPayload);
                    var claimsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes) ?? new Dictionary<string, object>();
                    
                    var claims = claimsDict.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? "")).ToList();
                    var identity = new ClaimsIdentity(claims, "custom");
                    user = new ClaimsPrincipal(identity);
                }
                else
                {
                    user = new ClaimsPrincipal(new ClaimsIdentity());
                }
            }
            else
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var claims = jwtToken.Claims.ToList();
                var identity = new ClaimsIdentity(claims, "jwt");
                user = new ClaimsPrincipal(identity);
            }
            
            var authState = Task.FromResult(new AuthenticationState(user));
            NotifyAuthenticationStateChanged(authState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao notificar autenticação");
        }
    }

    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        NotifyAuthenticationStateChanged(authState);
    }
}