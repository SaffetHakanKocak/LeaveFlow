using LeaveFlow.Application.Identity;
using LeaveFlow.Infrastructure.Identity;

namespace LeaveFlow.UnitTests.Identity;

public sealed class PasswordHashingTests
{
    [Fact]
    public void AspNetPasswordHasher_Should_HashAndVerify_WithoutStoringPlaintext()
    {
        var hasher = new AspNetPasswordHashingService();
        const string password = "Test.Passw0rd!";

        var hash = hasher.HashPassword(password);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
        Assert.Equal(PasswordVerificationOutcome.Success, hasher.Verify(hash, password));
        Assert.Equal(PasswordVerificationOutcome.Failed, hasher.Verify(hash, "other-password"));
    }
}
