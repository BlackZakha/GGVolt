using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GGVolt.Client.Models.Api;

namespace GGVolt.Client.Services;

public class TokenStorage : ITokenStorage
{
    private readonly string _filePath;
    private readonly ITokenAccessor _session;
    
    private static readonly byte[] _key = Convert.FromBase64String("xVz8K2mN9pQ4rS7tU1vW3xY5zA6bC8dE0fG2hI4jK6M=");

    public TokenStorage(ITokenAccessor session)
    {
        _session = session;
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "GGVolt");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "auth.dat");
    }

    public async Task SaveAsync(AuthResponse tokens)
    {
        var json = JsonSerializer.Serialize(tokens);
        var encrypted = Protect(Encoding.UTF8.GetBytes(json));
        await File.WriteAllBytesAsync(_filePath, encrypted);
        
        // ✅ Синхронизируем сессию
        _session.SetToken(tokens.AccessToken);
    }

    public async Task<AuthResponse?> LoadAsync()
    {
        if (!File.Exists(_filePath)) return null;
        try
        {
            var encrypted = await File.ReadAllBytesAsync(_filePath);
            var decrypted = Unprotect(encrypted);
            var tokens = JsonSerializer.Deserialize<AuthResponse>(decrypted);
            
            // ✅ Автоматически обновляем сессию при загрузке
            if (tokens != null)
            {
                _session.SetToken(tokens.AccessToken);
            }
            
            return tokens;
        }
        catch 
        { 
            return null; 
        }
    }

    public Task ClearAsync()
    {
        if (File.Exists(_filePath)) File.Delete(_filePath);
        
        // ✅ Очищаем сессию
        _session.SetToken(null);
        return Task.CompletedTask;
    }

    private byte[] Protect(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);
        ms.Write(encryptor.TransformFinalBlock(data, 0, data.Length), 0, data.Length);
        return ms.ToArray();
    }

    private byte[] Unprotect(byte[] data)
    {
        if (data.Length < 16) throw new CryptographicException("Invalid data");
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = data[..16];
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        return decryptor.TransformFinalBlock(data, 16, data.Length - 16);
    }
}