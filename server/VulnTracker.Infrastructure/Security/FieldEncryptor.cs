using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace VulnTracker.Infrastructure.Security;

// AES-256-GCM. Stored format: enc:<keyId>:<base64(nonce | ciphertext | tag)>
public sealed class FieldEncryptor
{
    private const string Prefix = "enc:";
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly Dictionary<string, byte[]> _keys = new();
    private readonly string _currentKeyId;

    public FieldEncryptor(IConfiguration config)
    {
        foreach (var k in config.GetSection("Crypto:Keys").GetChildren())
        {
            var key = Convert.FromBase64String(k.Value ?? "");
            if (key.Length != 32)
                throw new InvalidOperationException($"Crypto:Keys:{k.Key} must be 32 bytes in base64");
            _keys[k.Key] = key;
        }

        // fail closed: no key means the API does not start, instead of saving plain text
        _currentKeyId = config["Crypto:CurrentKeyId"]
            ?? throw new InvalidOperationException("Crypto:CurrentKeyId is not configured");
        if (!_keys.ContainsKey(_currentKeyId))
            throw new InvalidOperationException($"No key with id '{_currentKeyId}' in Crypto:Keys");
    }

    // context is the field name, it is authenticated but not stored
    public string Encrypt(string plaintext, string context)
    {
        var data = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize); // new nonce every time, never reuse one
        var cipher = new byte[data.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_keys[_currentKeyId], TagSize);
        aes.Encrypt(nonce, data, cipher, tag, Encoding.UTF8.GetBytes(context));

        var blob = new byte[NonceSize + cipher.Length + TagSize];
        nonce.CopyTo(blob, 0);
        cipher.CopyTo(blob, NonceSize);
        tag.CopyTo(blob, NonceSize + cipher.Length);
        return $"{Prefix}{_currentKeyId}:{Convert.ToBase64String(blob)}";
    }

    public string Decrypt(string stored, string context)
    {
        // rows from before encryption existed are plain text, they pass through
        if (!stored.StartsWith(Prefix)) return stored;

        var parts = stored.Split(':', 3);
        if (parts.Length != 3 || !_keys.TryGetValue(parts[1], out var key))
            throw new CryptographicException("Unknown key id in stored value");

        var blob = Convert.FromBase64String(parts[2]);
        var nonce = blob.AsSpan(0, NonceSize);
        var tag = blob.AsSpan(blob.Length - TagSize);
        var cipher = blob.AsSpan(NonceSize, blob.Length - NonceSize - TagSize);
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(key, TagSize);
        // throws if the data was changed, the key is wrong or the context does not match
        aes.Decrypt(nonce, cipher, tag, plain, Encoding.UTF8.GetBytes(context));
        return Encoding.UTF8.GetString(plain);
    }
}
