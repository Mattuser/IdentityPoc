using IdentityPoc.Api.Domain.Authorization;
using IdentityPoc.Api.Domain.Users;

namespace IdentityPoc.Api.Tests.Domain;

public sealed class UserAccountTests
{
    [Fact]
    public void GrantAddsDirectPermissionOnce()
    {
        var user = new UserAccount(Guid.NewGuid(), "Test User", "test@company.local", "secret", Role.User);

        user.Grant(Permission.Delete);
        user.Grant(Permission.Delete);

        Assert.Equal([Permission.Delete], user.DirectPermissions.ToArray());
    }

    [Fact]
    public void AddToGroupAddsGroupOnce()
    {
        var groupId = Guid.NewGuid();
        var user = new UserAccount(Guid.NewGuid(), "Test User", "test@company.local", "secret", Role.User);

        user.AddToGroup(groupId);
        user.AddToGroup(groupId);

        Assert.Equal([groupId], user.GroupIds.ToArray());
    }
}
