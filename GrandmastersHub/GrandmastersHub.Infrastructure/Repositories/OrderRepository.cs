using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Interfaces;
using GrandmastersHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly GrandmastersDbContext _context;

    public OrderRepository(GrandmastersDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.ProductVariant)
            .Include(o => o.Payment)
            .Include(o => o.Shipment)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.ProductVariant)
            .Include(o => o.Payment)
            .Include(o => o.Shipment)
            .FirstOrDefaultAsync(o => o.OrderId == id);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.ProductVariant)
            .Include(o => o.Payment)
            .Include(o => o.Shipment)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public async Task<Order> AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order is null)
        {
            return;
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
    }
}