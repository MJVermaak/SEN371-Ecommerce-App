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
    private readonly User _dummyUser = new() { Email = "dummy@invalid.local", PasswordHash = string.Empty };
    private readonly string _dummyPasswordHash;

    public AuthService(IUserRepository users, IPasswordService passwords, ITokenService tokens)
    {
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _dummyPasswordHash = passwords.Hash(_dummyUser, "not-a-real-account-password");
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        if (await _users.GetByEmailAsync(email, cancellationToken) is not null)
            return null;

        var user = new User
        {
            FirstName = string.Empty,
            LastName = string.Empty,
            Email = email,
            PasswordHash = string.Empty,
            Role = "Customer",
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwords.Hash(user, request.Password);
        try
        {
            await _users.AddAsync(user, cancellationToken);
            return _tokens.CreateToken(user);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return null;
        }
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        var passwordIsValid = user is null
            ? _passwords.Verify(_dummyUser, _dummyPasswordHash, request.Password)
            : _passwords.Verify(user, user.PasswordHash, request.Password);

        return user is null || !passwordIsValid ? null : _tokens.CreateToken(user);
    }

    public Task<User?> GetUserAsync(int userId, CancellationToken cancellationToken = default) =>
        _users.GetByIdAsync(userId, cancellationToken);
}
