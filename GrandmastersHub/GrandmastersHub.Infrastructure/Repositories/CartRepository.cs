using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Interfaces;
using GrandmastersHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly GrandmastersDbContext _context;

    public CartRepository(GrandmastersDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByIdAsync(int id)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.CartId == id);
    }

    public async Task<Cart?> GetByUserIdAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> AddAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        return cart;
    }

    public async Task UpdateAsync(Cart cart)
    {
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.CartId == id);

        if (cart is null)
        {
            return;
        }

        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync();
    }
}