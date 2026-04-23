using IdentityPoc.Api.Domain.Authorization;

namespace IdentityPoc.Api.Domain.Groups;

public sealed class AccessGroup
{
    public AccessGroup(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public Guid Id { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public HashSet<Permission> Permissions { get; } = [];
    public HashSet<Guid> UserIds { get; } = [];

    public void AddUser(Guid userId)
    {
        UserIds.Add(userId);
    }

    public void Grant(Permission permission)
    {
        Permissions.Add(permission);
    }
}
