using Fortune.Repository.Basic;
using Fortune.Repository.DBContext;
using Fortune.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Repository
{
    public class OrderRepository : GenericRepository<Order>
    {
        public OrderRepository(FortuneContext context) : base(context)
        {
        }
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders.ToListAsync();
        }
        public async Task<Order> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        }
        public async Task<Order> GetOrdersByOrderCodeAsync(long orderCode)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }
        public async Task<List<Order>> GetUnclaimedOrdersByEmailAsync(string email)
        {
            return await _context.Orders
                .Where(o => o.UserId == null && o.GuestEmail == email)
                .ToListAsync();
        }
        public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }
    }
}
