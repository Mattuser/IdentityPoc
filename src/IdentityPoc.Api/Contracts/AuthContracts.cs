using IdentityPoc.Api.Domain.Authorization;

namespace IdentityPoc.Api.Contracts;

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthenticatedUserResponse(
    Guid UserId,
    string DisplayName,
    string Email,
    Role Role,
    IReadOnlyCollection<Permission> Permissions);

public sealed record UserProfileResponse(
    Guid UserId,
    string DisplayName,
    string Email,
    Role Role,
    IReadOnlyCollection<Permission> Permissions,
    IReadOnlyCollection<GroupSummaryResponse> Groups);

public sealed record GroupSummaryResponse(Guid Id, string Name);
