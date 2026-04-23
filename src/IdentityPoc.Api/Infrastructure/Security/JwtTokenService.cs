using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IdentityPoc.Api.Domain.Authorization;
using IdentityPoc.Api.Domain.Users;
using Microsoft.Extensions.Options;

namespace IdentityPoc.Api.Infrastructure.Security;

public sealed class JwtTokenService
{
    private readonly JwtTokenOptions _options;

    public JwtTokenService(IOptions<JwtTokenOptions> options)
    {
        _options = options.Value;
    }

    public IssuedToken Issue(UserAccount user, IReadOnlyCollection<Permission> permissions)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(_options.AccessTokenLifetime);
        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object>
        {
            ["iss"] = _options.Issuer,
            ["aud"] = _options.Audience,
            ["sub"] = user.Id,
            ["email"] = user.Email,
            ["role"] = user.Role.ToString(),
            ["permissions"] = permissions.Select(permission => permission.ToString()).ToArray(),
            ["exp"] = expiresAt.ToUnixTimeSeconds(),
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var unsignedToken = $"{encodedHeader}.{encodedPayload}";
        var signature = Base64UrlEncode(Sign(unsignedToken));

        return new IssuedToken($"{unsignedToken}.{signature}", expiresAt);
    }

    public TokenValidationResult Validate(string token)
    {
        var parts = token.Split('.');

        if (parts.Length != 3)
        {
            return TokenValidationResult.Invalid;
        }

        var unsignedToken = $"{parts[0]}.{parts[1]}";
        var expectedSignature = Sign(unsignedToken);
        var actualSignature = Base64UrlDecode(parts[2]);

        if (!CryptographicOperations.FixedTimeEquals(actualSignature, expectedSignature))
        {
            return TokenValidationResult.Invalid;
        }

        using var payload = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        var root = payload.RootElement;

        if (!root.TryGetProperty("iss", out var issuer) ||
            issuer.GetString() != _options.Issuer ||
            !root.TryGetProperty("aud", out var audience) ||
            audience.GetString() != _options.Audience ||
            !root.TryGetProperty("exp", out var expiresAt) ||
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expiresAt.GetInt64() ||
            !root.TryGetProperty("sub", out var subject) ||
            !Guid.TryParse(subject.GetString(), out var userId))
        {
            return TokenValidationResult.Invalid;
        }

        return new TokenValidationResult(true, userId);
    }

    private byte[] Sign(string unsignedToken)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SigningKey));

        return hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken));
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value
            .Replace('-', '+')
            .Replace('_', '/');

        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');

        return Convert.FromBase64String(padded);
    }
}

public sealed record IssuedToken(string AccessToken, DateTimeOffset ExpiresAt);

public sealed record TokenValidationResult(bool IsValid, Guid? UserId)
{
    public static TokenValidationResult Invalid { get; } = new(false, null);
}
