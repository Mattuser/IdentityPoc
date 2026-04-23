namespace IdentityPoc.Api.Domain.Authorization;

public static class RolePermissionPolicy
{
    public static IReadOnlySet<Permission> GetPermissions(Role role)
    {
        return role switch
        {
            Role.User => new HashSet<Permission> { Permission.Read, Permission.Write },
            Role.Mod => new HashSet<Permission> { Permission.Read, Permission.Write, Permission.Delete, Permission.Update },
            Role.Admin => Enum.GetValues<Permission>().ToHashSet(),
            _ => new HashSet<Permission>()
        };
    }
}
