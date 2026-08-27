using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;
using Microsoft.AspNetCore.Identity;

namespace LeaveFlow.Infrastructure.Identity;

public sealed class AspNetPasswordHashingService : IPasswordHashingService
{
    private readonly PasswordHasher<object> _passwordHasher = new();
    private readonly string _dummyHash;

    public AspNetPasswordHashingService()
    {
        _dummyHash = _passwordHasher.HashPassword(new object(), "timing-protection-dummy");
    }

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return _passwordHasher.HashPassword(new object(), password);
    }

    public PasswordVerificationOutcome Verify(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || providedPassword is null)
        {
            return PasswordVerificationOutcome.Failed;
        }

        var result = _passwordHasher.VerifyHashedPassword(new object(), hashedPassword, providedPassword);
        return result switch
        {
            PasswordVerificationResult.Success => PasswordVerificationOutcome.Success,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordVerificationOutcome.SuccessRehashNeeded,
            _ => PasswordVerificationOutcome.Failed
        };
    }

    public void PerformDummyVerification(string providedPassword)
    {
        _ = _passwordHasher.VerifyHashedPassword(new object(), _dummyHash, providedPassword ?? string.Empty);
    }
}
