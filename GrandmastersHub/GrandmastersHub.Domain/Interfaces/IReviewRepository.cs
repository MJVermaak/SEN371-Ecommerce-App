using GrandmastersHub.Domain.Entities;

namespace GrandmastersHub.Domain.Interfaces;

public interface IReviewRepository
{
    Task<IEnumerable<Review>> GetByProductIdAsync(int productId);

    Task<IEnumerable<Review>> GetByUserIdAsync(int userId);

    Task<Review?> GetByIdAsync(int id);

    Task<Review> AddAsync(Review review);

    Task UpdateAsync(Review review);

    Task DeleteAsync(int id);
}