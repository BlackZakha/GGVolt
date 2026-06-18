using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GGVolt.Client.Models.Api;
using Microsoft.Extensions.Logging;

namespace GGVolt.Client.Services;

public class TokenStorage : ITokenStorage
{
    private readonly string _filePath;
    private readonly ITokenAccessor _session;
    private readonly ILogger<TokenStorage>? _logger;

    public TokenStorage(ITokenAccessor session, ILogger<TokenStorage>? logger = null)
    {
        _session = session;
        _logger = logger;
        
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "GGVolt");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "auth.dat");
        
        _logger?.LogInformation("📁 TokenStorage инициализирован: {Path}", _filePath);
        
        LoadTokenSynchronously();
    }

    private void LoadTokenSynchronously()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _logger?.LogInformation("⚠️ Файл токена не найден при старте");
                return;
            }

            _logger?.LogInformation("📖 Синхронная загрузка токена из {Path}", _filePath);
            var json = File.ReadAllText(_filePath);
            var tokens = JsonSerializer.Deserialize<AuthResponse>(json);
            
            if (tokens != null && !string.IsNullOrEmpty(tokens.AccessToken))
            {
                _session.SetTokens(tokens.AccessToken, tokens.RefreshToken);
                _logger?.LogInformation("✅ Токены загружены при старте, IsAuthenticated={Auth}", 
                    _session.IsAuthenticated);
            }
            else
            {
                _logger?.LogWarning("⚠️ Токен пустой или невалидный при старте");
                File.Delete(_filePath);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "❌ Ошибка при синхронной загрузке токена");
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                _logger?.LogInformation("🗑️ Повреждённый файл удалён");
            }
        }
    }

    public async Task SaveAsync(AuthResponse tokens)
    {
        try
        {
            if (tokens == null || string.IsNullOrEmpty(tokens.AccessToken))
            {
                _logger?.LogWarning("⚠️ Попытка сохранить пустой токен!");
                return;
            }

            _logger?.LogInformation("💾 Сохранение токенов...");
            
            var json = JsonSerializer.Serialize(tokens);
            await File.WriteAllTextAsync(_filePath, json);
            
            _session.SetTokens(tokens.AccessToken, tokens.RefreshToken);
            
            _logger?.LogInformation("✅ Токены сохранены, IsAuthenticated={Auth}", _session.IsAuthenticated);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "❌ Ошибка при сохранении токена");
            throw;
        }
    }

    public async Task<AuthResponse?> LoadAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _logger?.LogWarning("⚠️ Файл токена не найден");
                return null;
            }

            _logger?.LogInformation("📖 Загрузка токена из {Path}", _filePath);
            var json = await File.ReadAllTextAsync(_filePath);
            var tokens = JsonSerializer.Deserialize<AuthResponse>(json);
            
            if (tokens != null && !string.IsNullOrEmpty(tokens.AccessToken))
            {
                _session.SetTokens(tokens.AccessToken, tokens.RefreshToken);
                _logger?.LogInformation("✅ Токены загружены, IsAuthenticated={Auth}", _session.IsAuthenticated);
            }
            else
            {
                _logger?.LogWarning("⚠️ Токен пустой или невалидный");
                File.Delete(_filePath);
            }
            
            return tokens;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "❌ Ошибка при загрузке токена");
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                _logger?.LogInformation("🗑️ Повреждённый файл удалён");
            }
            return null;
        }
    }

    public Task ClearAsync()
    {
        try
        {
            _logger?.LogInformation("🗑️ Очистка токенов");
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                _logger?.LogInformation("✅ Файл удален");
            }
            
            _session.ClearTokens();
            _logger?.LogInformation("✅ Сессия очищена, IsAuthenticated={Auth}", _session.IsAuthenticated);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "❌ Ошибка при очистке токена");
        }
        return Task.CompletedTask;
    }
}