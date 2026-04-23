using IdentityPoc.Api.Domain.Authorization;

namespace IdentityPoc.Api.Tests.Domain;

public sealed class RolePermissionPolicyTests
{
    [Fact]
    public void UserRoleIncludesReadAndWriteOnly()
    {
        var permissions = RolePermissionPolicy.GetPermissions(Role.User);

        Assert.Equal([Permission.Read, Permission.Write], permissions.Order().ToArray());
    }

    [Fact]
    public void ModRoleIncludesReadWriteDeleteAndUpdateOnly()
    {
        var permissions = RolePermissionPolicy.GetPermissions(Role.Mod);

        Assert.Equal(
            [Permission.Read, Permission.Write, Permission.Delete, Permission.Update],
            permissions.Order().ToArray());
    }

    [Fact]
    public void AdminRoleIncludesAllPermissions()
    {
        var permissions = RolePermissionPolicy.GetPermissions(Role.Admin);

        Assert.Equal(Enum.GetValues<Permission>().Order().ToArray(), permissions.Order().ToArray());
    }
}
