using GrandmastersHub.Application.DTOs.Auth;
using GrandmastersHub.Application.Services;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Interfaces;
using GrandmastersHub.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GrandmastersHub.Tests;

public sealed class AuthServiceTests
{
    private const string SigningKey = "test-only-signing-key-with-at-least-32-bytes";

    [Fact]
    public async Task RegisterAsync_StoresAHashNotThePassword()
    {
        var repository = new InMemoryUserRepository();
        var service = CreateService(repository);

        var response = await service.RegisterAsync(new RegisterRequestDto { Email = "  Player.One@Example.com  ", Password = "correct horse battery staple" });

        var stored = Assert.Single(repository.Users);
        Assert.NotNull(response);
        Assert.Equal("Player.One@Example.com", stored.Email);
        Assert.NotEqual("correct horse battery staple", stored.PasswordHash);
        Assert.NotEmpty(stored.PasswordHash);
        Assert.True(stored.UserId > 0);
        Assert.Equal(stored.UserId, response.UserId);
        Assert.Equal(stored.Email, response.Email);
    }

    [Fact]
    public async Task RegisterAsync_RejectsAnExistingEmailRegardlessOfCaseOrWhitespace()
    {
        var repository = new InMemoryUserRepository();
        var service = CreateService(repository);
        await service.RegisterAsync(new RegisterRequestDto { Email = "player@example.com", Password = "first secure password" });

        var duplicate = await service.RegisterAsync(new RegisterRequestDto { Email = "player@example.com", Password = "second secure password" });

        Assert.Null(duplicate);
        Assert.Single(repository.Users);
    }

    [Fact]
    public async Task LoginAsync_AcceptsTheCorrectPassword()
    {
        var repository = new InMemoryUserRepository();
        var service = CreateService(repository);
        var registration = await service.RegisterAsync(new RegisterRequestDto { Email = "player@example.com", Password = "correct horse battery staple" });

        var login = await service.LoginAsync(new LoginRequestDto { Email = "  player@example.com ", Password = "correct horse battery staple" });

        Assert.NotNull(login);
        Assert.Equal(registration!.UserId, login.UserId);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNullForWrongPasswordAndUnknownEmail()
    {
        var repository = new InMemoryUserRepository();
        var service = CreateService(repository);
        await service.RegisterAsync(new RegisterRequestDto { Email = "player@example.com", Password = "correct horse battery staple" });

        Assert.Null(await service.LoginAsync(new LoginRequestDto { Email = "player@example.com", Password = "incorrect password" }));
        Assert.Null(await service.LoginAsync(new LoginRequestDto { Email = "unknown@example.com", Password = "incorrect password" }));
    }

    private static AuthService CreateService(InMemoryUserRepository repository)
    {
        var passwordService = new PasswordService(new PasswordHasher<User>());
        var tokenService = new JwtTokenService(Options.Create(new JwtOptions
        {
            Issuer = "GrandmastersHub.Tests", Audience = "GrandmastersHub.Tests.Client", SigningKey = SigningKey, ExpiryMinutes = 15
        }));
        return new AuthService(repository, passwordService, tokenService);
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = [];
        public IReadOnlyList<User> Users => _users;

        public Task<IEnumerable<User>> GetAllAsync() => Task.FromResult<IEnumerable<User>>(_users);
        public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(_users.SingleOrDefault(u => u.UserId == id));
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(_users.SingleOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

        public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user.UserId == 0) user.UserId = _users.Count + 1;
            _users.Add(user);
            return Task.FromResult(user);
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(int id, CancellationToken cancellationToken = default) { _users.RemoveAll(u => u.UserId == id); return Task.CompletedTask; }
    }
}
