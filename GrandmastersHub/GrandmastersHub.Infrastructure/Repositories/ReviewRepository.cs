using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Interfaces;
using GrandmastersHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly GrandmastersDbContext _context;

    public ReviewRepository(GrandmastersDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.ProductId == productId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByUserIdAsync(int userId)
    {
        return await _context.Reviews
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.ReviewId == id);
    }

    public async Task<Review> AddAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();

        return review;
    }

    public async Task UpdateAsync(Review review)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ReviewId == id);

        if (review is null)
        {
            return;
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }
}