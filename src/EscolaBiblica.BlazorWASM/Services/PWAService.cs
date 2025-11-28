using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System.Text.Json;

namespace EscolaBiblica.BlazorWASM.Services;

public interface IPWAService
{
    Task<bool> IsOnlineAsync();
    Task<bool> IsInstallableAsync();
    Task InstallPWAAsync();
    Task RegisterForBackgroundSyncAsync(string tag);
    Task ShowNotificationAsync(string title, string message);
    Task<bool> RequestNotificationPermissionAsync();
    Task<T?> GetOfflineDataAsync<T>(string key, TimeSpan? maxAge = null);
    Task SetOfflineDataAsync<T>(string key, T data);
    Task SyncOfflineDataAsync();
}

public class PWAService : IPWAService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<PWAService> _logger;
    private const string OfflineQueueKey = "offline_queue";
    private const string LastSyncKey = "last_sync";

    public PWAService(IJSRuntime jsRuntime, ILocalStorageService localStorage, ILogger<PWAService> logger)
    {
        _jsRuntime = jsRuntime;
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<bool> IsOnlineAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("PWA.isOnline");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar status online");
            return true; // Assume online se não conseguir verificar
        }
    }

    public async Task<bool> IsInstallableAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("PWA.isInstallable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar se PWA é instalável");
            return false;
        }
    }

    public async Task InstallPWAAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("PWA.installPWA");
            _logger.LogInformation("PWA instalado pelo usuário");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao instalar PWA");
        }
    }

    public async Task RegisterForBackgroundSyncAsync(string tag)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("PWA.registerBackgroundSync", tag);
            _logger.LogDebug("Background sync registrado para tag: {Tag}", tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar background sync para tag: {Tag}", tag);
        }
    }

    public async Task<bool> RequestNotificationPermissionAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("PWA.requestNotificationPermission");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao solicitar permissão de notificação");
            return false;
        }
    }

    public async Task ShowNotificationAsync(string title, string message)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("PWA.showNotification", title, new { body = message });
            _logger.LogDebug("Notificação exibida: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao exibir notificação: {Title}", title);
        }
    }

    public async Task<T?> GetOfflineDataAsync<T>(string key, TimeSpan? maxAge = null)
    {
        try
        {
            var maxAgeMs = maxAge?.TotalMilliseconds ?? 86400000; // 24h padrão
            var dataJson = await _jsRuntime.InvokeAsync<string?>("PWA.getOfflineData", key, maxAgeMs);
            
            if (string.IsNullOrEmpty(dataJson))
                return default;

            return JsonSerializer.Deserialize<T>(dataJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao recuperar dados offline para chave: {Key}", key);
            return default;
        }
    }

    public async Task SetOfflineDataAsync<T>(string key, T data)
    {
        try
        {
            var dataJson = JsonSerializer.Serialize(data);
            await _jsRuntime.InvokeVoidAsync("PWA.storeOfflineData", key, dataJson);
            _logger.LogDebug("Dados armazenados offline para chave: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao armazenar dados offline para chave: {Key}", key);
        }
    }

    public async Task SyncOfflineDataAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando sincronização de dados offline");

            // Recuperar fila de sincronização
            var offlineQueue = await GetOfflineQueueAsync();
            
            if (!offlineQueue.Any())
            {
                _logger.LogDebug("Nenhum dado offline para sincronizar");
                return;
            }

            var successCount = 0;
            var failedItems = new List<OfflineQueueItem>();

            foreach (var item in offlineQueue)
            {
                try
                {
                    await ProcessOfflineItemAsync(item);
                    successCount++;
                    _logger.LogDebug("Item sincronizado com sucesso: {Type} - {Id}", item.Type, item.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao sincronizar item: {Type} - {Id}", item.Type, item.Id);
                    failedItems.Add(item);
                }
            }

            // Atualizar fila apenas com itens que falharam
            await SetOfflineQueueAsync(failedItems);
            
            // Registrar último sync
            await _localStorage.SetItemAsync(LastSyncKey, DateTime.UtcNow);

            _logger.LogInformation("Sincronização concluída: {Success} sucessos, {Failed} falhas", 
                successCount, failedItems.Count);

            if (successCount > 0)
            {
                await ShowNotificationAsync("Sincronização Concluída", 
                    $"{successCount} itens sincronizados com sucesso");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante sincronização de dados offline");
        }
    }

    public async Task QueueOfflineActionAsync(string type, string id, object data)
    {
        try
        {
            var queue = await GetOfflineQueueAsync();
            
            queue.Add(new OfflineQueueItem
            {
                Id = id,
                Type = type,
                Data = JsonSerializer.Serialize(data),
                Timestamp = DateTime.UtcNow
            });

            await SetOfflineQueueAsync(queue);
            
            _logger.LogDebug("Ação offline enfileirada: {Type} - {Id}", type, id);

            // Tentar sincronizar imediatamente se estiver online
            if (await IsOnlineAsync())
            {
                await SyncOfflineDataAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enfileirar ação offline: {Type} - {Id}", type, id);
        }
    }

    private async Task<List<OfflineQueueItem>> GetOfflineQueueAsync()
    {
        try
        {
            var queueJson = await _localStorage.GetItemAsStringAsync(OfflineQueueKey);
            if (string.IsNullOrEmpty(queueJson))
                return new List<OfflineQueueItem>();

            return JsonSerializer.Deserialize<List<OfflineQueueItem>>(queueJson) ?? new List<OfflineQueueItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao recuperar fila offline");
            return new List<OfflineQueueItem>();
        }
    }

    private async Task SetOfflineQueueAsync(List<OfflineQueueItem> queue)
    {
        try
        {
            var queueJson = JsonSerializer.Serialize(queue);
            await _localStorage.SetItemAsStringAsync(OfflineQueueKey, queueJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar fila offline");
        }
    }

    private async Task ProcessOfflineItemAsync(OfflineQueueItem item)
    {
        // Este método seria implementado com base nos tipos de ação
        // Por exemplo: criar aluno, registrar presença, etc.
        
        switch (item.Type.ToLowerInvariant())
        {
            case "presenca":
                // Processar registro de presença offline
                _logger.LogDebug("Processando presença offline: {Id}", item.Id);
                break;
                
            case "aluno":
                // Processar criação/edição de aluno offline
                _logger.LogDebug("Processando aluno offline: {Id}", item.Id);
                break;
                
            default:
                _logger.LogWarning("Tipo de item offline não reconhecido: {Type}", item.Type);
                break;
        }
        
        // Simular processamento por enquanto
        await Task.Delay(100);
    }

    public async Task<DateTime?> GetLastSyncTimeAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<DateTime?>(LastSyncKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao recuperar último tempo de sincronização");
            return null;
        }
    }

    public async Task CleanupExpiredDataAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("PWA.cleanupOfflineData");
            _logger.LogDebug("Limpeza de dados offline expirados executada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante limpeza de dados offline");
        }
    }
}

public class OfflineQueueItem
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}