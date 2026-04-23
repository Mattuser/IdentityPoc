using IdentityPoc.Api.Infrastructure.Security;

namespace IdentityPoc.Api.Tests.Integration;

public sealed class PasswordHasherTests
{
    [Fact]
    public void HashUsesBcryptByDefault()
    {
        var passwordHasher = new PasswordHasher();

        var passwordHash = passwordHasher.Hash("secret");

        Assert.StartsWith("$2", passwordHash);
        Assert.True(passwordHasher.Verify("secret", passwordHash));
    }

    [Fact]
    public void VerifyAcceptsPbkdf2Hashes()
    {
        var passwordHasher = new PasswordHasher();
        var passwordHash = passwordHasher.HashWithPbkdf2("secret");

        Assert.StartsWith("pbkdf2-sha256.", passwordHash);
        Assert.True(passwordHasher.Verify("secret", passwordHash));
    }
}
