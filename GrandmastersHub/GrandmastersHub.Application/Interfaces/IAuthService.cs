using GrandmastersHub.Application.DTOs.Auth;
using GrandmastersHub.Domain.Entities;

namespace GrandmastersHub.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<User?> GetUserAsync(int userId, CancellationToken cancellationToken = default);
}

public interface IPasswordService
{
    string Hash(User user, string password);
    bool Verify(User user, string passwordHash, string password);
}

public interface ITokenService
{
    AuthResponseDto CreateToken(User user);
}
