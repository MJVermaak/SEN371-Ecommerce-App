using GrandmastersHub.Application.Interfaces;
using GrandmastersHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GrandmastersHub.Infrastructure.Security;

public sealed class PasswordService(IPasswordHasher<User> passwordHasher) : IPasswordService
{
    public string Hash(User user, string password) => passwordHasher.HashPassword(user, password);

    public bool Verify(User user, string passwordHash, string password) =>
        passwordHasher.VerifyHashedPassword(user, passwordHash, password) is not PasswordVerificationResult.Failed;
}
