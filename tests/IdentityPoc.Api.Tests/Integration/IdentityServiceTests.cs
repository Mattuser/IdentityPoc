using IdentityPoc.Api.Contracts;
using IdentityPoc.Api.Domain.Authorization;
using IdentityPoc.Api.Infrastructure;
using IdentityPoc.Api.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace IdentityPoc.Api.Tests.Integration;

public sealed class IdentityServiceTests
{
    [Fact]
    public void AuthenticateReturnsEffectivePermissionsForAdmin()
    {
        var service = CreateService();

        var authenticated = service.Authenticate(new LoginRequest("admin@company.local", "admin123"));

        Assert.NotNull(authenticated);
        Assert.Equal(Role.Admin, authenticated.Role);
        Assert.Equal("Bearer", authenticated.TokenType);
        Assert.False(string.IsNullOrWhiteSpace(authenticated.AccessToken));
        Assert.Contains(Permission.ManagePermissions, authenticated.Permissions);
        Assert.Equal(Enum.GetValues<Permission>().Order().ToArray(), authenticated.Permissions.Order().ToArray());
    }

    [Fact]
    public void AuthenticateReturnsValidJwtForAuthenticatedUser()
    {
        var tokenService = CreateTokenService();
        var service = CreateService(tokenService);

        var authenticated = service.Authenticate(new LoginRequest("admin@company.local", "admin123"));

        Assert.NotNull(authenticated);

        var validation = tokenService.Validate(authenticated.AccessToken);

        Assert.True(validation.IsValid);
        Assert.Equal(authenticated.UserId, validation.UserId);
    }

    [Fact]
    public void AuthenticateRejectsInvalidPassword()
    {
        var service = CreateService();

        var authenticated = service.Authenticate(new LoginRequest("admin@company.local", "wrong-password"));

        Assert.Null(authenticated);
    }

    [Fact]
    public void StoreKeepsPasswordHashInsteadOfPlainTextPassword()
    {
        var passwordHasher = new PasswordHasher();
        var store = new IdentityStore(passwordHasher);

        var user = store.FindUserByEmail("admin@company.local");

        Assert.NotNull(user);
        Assert.NotEqual("admin123", user.PasswordHash);
        Assert.True(passwordHasher.Verify("admin123", user.PasswordHash));
    }

    [Fact]
    public void AuthorizeUsesRolePermissions()
    {
        var service = CreateService();
        var user = service.Authenticate(new LoginRequest("user@company.local", "user123"));

        Assert.NotNull(user);

        var readAuthorization = service.Authorize(new AuthorizationRequest(user.UserId, Permission.Read));
        var deleteAuthorization = service.Authorize(new AuthorizationRequest(user.UserId, Permission.Delete));

        Assert.NotNull(readAuthorization);
        Assert.NotNull(deleteAuthorization);
        Assert.True(readAuthorization.IsAllowed);
        Assert.False(deleteAuthorization.IsAllowed);
    }

    [Fact]
    public void GrantPermissionToUserChangesAuthorizationResult()
    {
        var service = CreateService();
        var user = service.CreateUser(new CreateUserRequest("Temporary User", "temp@company.local", "temp123", Role.User));

        var beforeGrant = service.Authorize(new AuthorizationRequest(user.Id, Permission.Delete));

        var granted = service.GrantPermissionToUser(user.Id, Permission.Delete);
        var afterGrant = service.Authorize(new AuthorizationRequest(user.Id, Permission.Delete));

        Assert.NotNull(beforeGrant);
        Assert.NotNull(afterGrant);
        Assert.False(beforeGrant.IsAllowed);
        Assert.True(granted);
        Assert.True(afterGrant.IsAllowed);
    }

    [Fact]
    public void GroupPermissionsContributeToUserAuthorization()
    {
        var service = CreateService();
        var user = service.CreateUser(new CreateUserRequest("Grouped User", "grouped@company.local", "grouped123", Role.User));
        var group = service.CreateGroup(new CreateGroupRequest("Editors", "Users allowed to update records"));

        service.GrantPermissionToGroup(group.Id, Permission.Update);
        service.AddUserToGroup(user.Id, group.Id);

        var profile = service.GetProfile(user.Id);
        var authorization = service.Authorize(new AuthorizationRequest(user.Id, Permission.Update));

        Assert.NotNull(profile);
        Assert.NotNull(authorization);
        Assert.Contains(profile.Groups, item => item.Id == group.Id);
        Assert.True(authorization.IsAllowed);
    }

    [Fact]
    public void IsAdminReturnsTrueOnlyForAdminUsers()
    {
        var service = CreateService();
        var admin = service.Authenticate(new LoginRequest("admin@company.local", "admin123"));
        var user = service.Authenticate(new LoginRequest("user@company.local", "user123"));

        Assert.NotNull(admin);
        Assert.NotNull(user);
        Assert.True(service.IsAdmin(admin.UserId));
        Assert.False(service.IsAdmin(user.UserId));
    }

    private static IdentityService CreateService()
    {
        return CreateService(CreateTokenService());
    }

    private static IdentityService CreateService(JwtTokenService tokenService)
    {
        var passwordHasher = new PasswordHasher();

        return new IdentityService(new IdentityStore(passwordHasher), passwordHasher, tokenService);
    }

    private static JwtTokenService CreateTokenService()
    {
        return new JwtTokenService(Options.Create(new JwtTokenOptions()));
    }
}
