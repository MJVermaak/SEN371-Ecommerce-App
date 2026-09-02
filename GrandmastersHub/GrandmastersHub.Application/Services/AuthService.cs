using GrandmastersHub.Application.DTOs.Auth;
using GrandmastersHub.Application.Interfaces;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Interfaces;

namespace GrandmastersHub.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;
    private readonly ITokenService _tokens;
    private readonly User _dummyUser;
    private readonly string _dummyPasswordHash;

    public AuthService(IUserRepository users, IPasswordService passwords, ITokenService tokens)
    {
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _dummyUser = new User
        {
            Id = Guid.Empty,
            Email = "dummy@invalid.local",
            NormalizedEmail = "DUMMY@INVALID.LOCAL",
            PasswordHash = string.Empty,
            CreatedAtUtc = DateTimeOffset.UnixEpoch
        };
        _dummyPasswordHash = passwords.Hash(_dummyUser, "not-a-real-account-password");
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var normalizedEmail = NormalizeEmail(email);

        if (await _users.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = string.Empty,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        user.PasswordHash = _passwords.Hash(user, request.Password);

        return await _users.TryAddAsync(user, cancellationToken)
            ? _tokens.CreateToken(user)
            : null;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByNormalizedEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        var passwordIsValid = user is null
            ? _passwords.Verify(_dummyUser, _dummyPasswordHash, request.Password)
            : _passwords.Verify(user, user.PasswordHash, request.Password);

        if (user is null || !passwordIsValid)
        {
            return null;
        }

        return _tokens.CreateToken(user);
    }

    public Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
            _users.GetByIdAsync(userId, cancellationToken);

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
