using IdentityPoc.Api.Domain.Authorization;

namespace IdentityPoc.Api.Contracts;

public sealed record CreateUserRequest(string DisplayName, string Email, string Password, Role Role);

public sealed record CreateGroupRequest(string Name, string Description);

public sealed record AddUserToGroupRequest(Guid UserId);

public sealed record GrantPermissionRequest(Permission Permission);

public sealed record UserResponse(
    Guid Id,
    string DisplayName,
    string Email,
    Role Role,
    IReadOnlyCollection<Permission> DirectPermissions,
    IReadOnlyCollection<Guid> GroupIds);

public sealed record GroupResponse(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyCollection<Permission> Permissions,
    IReadOnlyCollection<Guid> UserIds);
