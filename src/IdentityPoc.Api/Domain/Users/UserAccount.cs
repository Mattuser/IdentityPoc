using IdentityPoc.Api.Domain.Authorization;

namespace IdentityPoc.Api.Domain.Users;

public sealed class UserAccount
{
    public UserAccount(Guid id, string displayName, string email, string password, Role role)
    {
        Id = id;
        DisplayName = displayName;
        Email = email;
        Password = password;
        Role = role;
    }

    public Guid Id { get; }
    public string DisplayName { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }
    public Role Role { get; private set; }
    public HashSet<Permission> DirectPermissions { get; } = [];
    public HashSet<Guid> GroupIds { get; } = [];

    public void AddToGroup(Guid groupId)
    {
        GroupIds.Add(groupId);
    }

    public void Grant(Permission permission)
    {
        DirectPermissions.Add(permission);
    }
}
