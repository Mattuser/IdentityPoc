using IdentityPoc.Api.Domain.Authorization;

namespace IdentityPoc.Api.Contracts;

public sealed record AuthorizationRequest(Guid UserId, Permission Permission);

public sealed record AuthorizationResponse(
    Guid UserId,
    Permission Permission,
    bool IsAllowed,
    IReadOnlyCollection<Permission> EffectivePermissions);
