using Sharply.Client.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class TokenStorageService : ITokenStorageService
{
    private string? _token;
    private readonly string EncryptionKey = "set-this-up-upon-initializtion-at-some-point-please";

    public void SaveToken(string token)
    {
        if (token == null)
            ClearTokenInternal();
        else
            SaveTokenInternal(token);
    }
    public async Task SaveTokenAsync(string token)
    {
        if (token == null)
            ClearTokenInternal();
        else
            await SaveTokenAsyncInternal(token);
    }

    public string? LoadToken() => _token ??= LoadTokenInternal();


    private void SaveTokenInternal(string token)
    {
        var encryptedToken = Encrypt(token, EncryptionKey);
        var filePath = GetTokenFilePath();
        File.WriteAllText(filePath, encryptedToken);
    }

    public async Task SaveTokenAsyncInternal(string token)
    {
        var encyptedToken = Encrypt(token, EncryptionKey);
        var filePath = GetTokenFilePath();
        await File.WriteAllTextAsync(filePath, encyptedToken);
    }

    private string? LoadTokenInternal()
    {
        var filePath = GetTokenFilePath();
        if (!File.Exists(filePath))
            return null;

        var encryptedToken = File.ReadAllText(filePath);
        return Decrypt(encryptedToken, EncryptionKey);
    }

    private void ClearTokenInternal()
    {
        var filePath = GetTokenFilePath();
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    private string GetTokenFilePath()
    {
        var appDataPath = Environment.OSVersion.Platform == PlatformID.Win32NT
            ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

        var appFolder = Path.Combine(appDataPath, "Sharply");
        Directory.CreateDirectory(appFolder);
        return Path.Combine(appFolder, "token.txt");
    }

    private string Encrypt(string plainText, string key)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32)); // Ensure 32-byte key
        aes.IV = new byte[16]; // Initialization vector (use a random IV for higher security)

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            using var sw = new StreamWriter(cs);
            sw.Write(plainText);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    private string Decrypt(string cipherText, string key)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
        aes.IV = new byte[16];

        var buffer = Convert.FromBase64String(cipherText);
        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(buffer);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}

