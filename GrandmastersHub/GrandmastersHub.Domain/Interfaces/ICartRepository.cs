using GrandmastersHub.Domain.Entities;

namespace GrandmastersHub.Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(int id);

    Task<Cart?> GetByUserIdAsync(int userId);

    Task<Cart> AddAsync(Cart cart);

    Task UpdateAsync(Cart cart);

    Task DeleteAsync(int id);

    // Load and save one user's cart atomically. Serialize writes for that user.
    Task<Cart> MutateAsync(int userId, Func<Cart, Task> mutation);
}