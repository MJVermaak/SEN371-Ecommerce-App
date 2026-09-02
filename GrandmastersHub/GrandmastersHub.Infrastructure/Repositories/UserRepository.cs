using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Interfaces;
using GrandmastersHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly GrandmastersDbContext _context;

    public UserRepository(GrandmastersDbContext context) => _context = context;

    public async Task<IEnumerable<User>> GetAllAsync() => await _context.Users
        .Include(u => u.Addresses).Include(u => u.Orders).Include(u => u.Reviews).ToListAsync();

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await _context.Users
        .Include(u => u.Addresses).Include(u => u.Orders).Include(u => u.Reviews).Include(u => u.Cart)
        .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.AsNoTracking().SingleOrDefaultAsync(
            u => u.Email.ToLower() == email.Trim().ToLower(),
            cancellationToken);

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([id], cancellationToken);
        if (user is null) return;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
