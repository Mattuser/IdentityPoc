using IdentityPoc.Api.Domain.Authorization;
using IdentityPoc.Api.Domain.Groups;
using IdentityPoc.Api.Domain.Users;

namespace IdentityPoc.Api.Infrastructure;

public sealed class IdentityStore
{
    private readonly Dictionary<Guid, UserAccount> _users = [];
    private readonly Dictionary<Guid, AccessGroup> _groups = [];

    public IdentityStore()
    {
        var moderator = AddUser("Support Moderator", "mod@company.local", "mod123", Role.Mod);
        var user = AddUser("Product User", "user@company.local", "user123", Role.User);

        AddUser("Platform Admin", "admin@company.local", "admin123", Role.Admin);

        var supportGroup = AddGroup("Support", "Users that can support customer identity requests.");
        supportGroup.Grant(Permission.Read);
        supportGroup.Grant(Permission.Update);
        AddUserToGroup(moderator.Id, supportGroup.Id);

        var internalAppsGroup = AddGroup("InternalApps", "Default group for trusted internal systems.");
        internalAppsGroup.Grant(Permission.Read);
        AddUserToGroup(user.Id, internalAppsGroup.Id);
    }

    public IReadOnlyCollection<UserAccount> Users => _users.Values;
    public IReadOnlyCollection<AccessGroup> Groups => _groups.Values;

    public UserAccount AddUser(string displayName, string email, string password, Role role)
    {
        var user = new UserAccount(Guid.NewGuid(), displayName, email, password, role);
        _users.Add(user.Id, user);

        return user;
    }

    public AccessGroup AddGroup(string name, string description)
    {
        var group = new AccessGroup(Guid.NewGuid(), name, description);
        _groups.Add(group.Id, group);

        return group;
    }

    public UserAccount? FindUser(Guid id)
    {
        return _users.GetValueOrDefault(id);
    }

    public UserAccount? FindUserByEmail(string email)
    {
        return _users.Values.FirstOrDefault(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public AccessGroup? FindGroup(Guid id)
    {
        return _groups.GetValueOrDefault(id);
    }

    public bool AddUserToGroup(Guid userId, Guid groupId)
    {
        var user = FindUser(userId);
        var group = FindGroup(groupId);

        if (user is null || group is null)
        {
            return false;
        }

        user.AddToGroup(groupId);
        group.AddUser(userId);

        return true;
    }
}
