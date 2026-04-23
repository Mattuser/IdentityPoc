using IdentityPoc.Api.Contracts;
using IdentityPoc.Api.Domain.Authorization;
using IdentityPoc.Api.Domain.Groups;
using IdentityPoc.Api.Domain.Users;
using IdentityPoc.Api.Infrastructure.Security;

namespace IdentityPoc.Api.Infrastructure;

public sealed class IdentityService
{
    private readonly IdentityStore _store;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenService _tokenService;

    public IdentityService(IdentityStore store, PasswordHasher passwordHasher, JwtTokenService tokenService)
    {
        _store = store;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public AuthenticatedUserResponse? Authenticate(LoginRequest request)
    {
        var user = _store.FindUserByEmail(request.Email);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var permissions = GetEffectivePermissions(user).Order().ToArray();
        var token = _tokenService.Issue(user, permissions);

        return new AuthenticatedUserResponse(
            user.Id,
            user.DisplayName,
            user.Email,
            user.Role,
            permissions,
            token.AccessToken,
            "Bearer",
            token.ExpiresAt);
    }

    public UserProfileResponse? GetProfile(Guid userId)
    {
        var user = _store.FindUser(userId);

        if (user is null)
        {
            return null;
        }

        return new UserProfileResponse(
            user.Id,
            user.DisplayName,
            user.Email,
            user.Role,
            GetEffectivePermissions(user).Order().ToArray(),
            GetUserGroups(user).Select(group => new GroupSummaryResponse(group.Id, group.Name)).ToArray());
    }

    public AuthorizationResponse? Authorize(AuthorizationRequest request)
    {
        var user = _store.FindUser(request.UserId);

        if (user is null)
        {
            return null;
        }

        var permissions = GetEffectivePermissions(user);

        return new AuthorizationResponse(
            user.Id,
            request.Permission,
            permissions.Contains(request.Permission),
            permissions.Order().ToArray());
    }

    public UserResponse CreateUser(CreateUserRequest request)
    {
        var user = _store.AddUser(request.DisplayName, request.Email, request.Password, request.Role);

        return ToUserResponse(user);
    }

    public GroupResponse CreateGroup(CreateGroupRequest request)
    {
        var group = _store.AddGroup(request.Name, request.Description);

        return ToGroupResponse(group);
    }

    public bool AddUserToGroup(Guid userId, Guid groupId)
    {
        return _store.AddUserToGroup(userId, groupId);
    }

    public bool GrantPermissionToUser(Guid userId, Permission permission)
    {
        var user = _store.FindUser(userId);

        if (user is null)
        {
            return false;
        }

        user.Grant(permission);

        return true;
    }

    public bool GrantPermissionToGroup(Guid groupId, Permission permission)
    {
        var group = _store.FindGroup(groupId);

        if (group is null)
        {
            return false;
        }

        group.Grant(permission);

        return true;
    }

    public IReadOnlyCollection<UserResponse> ListUsers()
    {
        return _store.Users.Select(ToUserResponse).ToArray();
    }

    public IReadOnlyCollection<GroupResponse> ListGroups()
    {
        return _store.Groups.Select(ToGroupResponse).ToArray();
    }

    public bool IsAdmin(Guid actorUserId)
    {
        var actor = _store.FindUser(actorUserId);

        if (actor is null)
        {
            return false;
        }

        return GetEffectivePermissions(actor).Contains(Permission.ManagePermissions);
    }

    private HashSet<Permission> GetEffectivePermissions(UserAccount user)
    {
        var permissions = RolePermissionPolicy.GetPermissions(user.Role).ToHashSet();

        permissions.UnionWith(user.DirectPermissions);

        foreach (var group in GetUserGroups(user))
        {
            permissions.UnionWith(group.Permissions);
        }

        return permissions;
    }

    private IEnumerable<AccessGroup> GetUserGroups(UserAccount user)
    {
        return user.GroupIds
            .Select(_store.FindGroup)
            .OfType<AccessGroup>();
    }

    private static UserResponse ToUserResponse(UserAccount user)
    {
        return new UserResponse(
            user.Id,
            user.DisplayName,
            user.Email,
            user.Role,
            user.DirectPermissions.Order().ToArray(),
            user.GroupIds.ToArray());
    }

    private static GroupResponse ToGroupResponse(AccessGroup group)
    {
        return new GroupResponse(
            group.Id,
            group.Name,
            group.Description,
            group.Permissions.Order().ToArray(),
            group.UserIds.ToArray());
    }
}
