using LeaveFlow.Application.Identity;

namespace LeaveFlow.Application.Abstractions.Identity;

public interface IPasswordHashingService
{
    string HashPassword(string password);

    PasswordVerificationOutcome Verify(string hashedPassword, string providedPassword);

    void PerformDummyVerification(string providedPassword);
}
