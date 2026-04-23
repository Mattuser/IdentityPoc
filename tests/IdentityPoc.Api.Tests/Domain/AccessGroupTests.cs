using IdentityPoc.Api.Domain.Authorization;
using IdentityPoc.Api.Domain.Groups;

namespace IdentityPoc.Api.Tests.Domain;

public sealed class AccessGroupTests
{
    [Fact]
    public void GrantAddsPermissionOnce()
    {
        var group = new AccessGroup(Guid.NewGuid(), "Support", "Support team");

        group.Grant(Permission.Update);
        group.Grant(Permission.Update);

        Assert.Equal([Permission.Update], group.Permissions.ToArray());
    }

    [Fact]
    public void AddUserAddsUserOnce()
    {
        var userId = Guid.NewGuid();
        var group = new AccessGroup(Guid.NewGuid(), "Support", "Support team");

        group.AddUser(userId);
        group.AddUser(userId);

        Assert.Equal([userId], group.UserIds.ToArray());
    }
}
