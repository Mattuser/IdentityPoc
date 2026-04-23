namespace IdentityPoc.Api.Infrastructure.Security;

public sealed class JwtTokenOptions
{
    public string Issuer { get; init; } = "IdentityPoc";
    public string Audience { get; init; } = "CompanySystems";
    public string SigningKey { get; init; } = "identity-poc-development-signing-key-change-before-production";
    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromHours(1);
}
