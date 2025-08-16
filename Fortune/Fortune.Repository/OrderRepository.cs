using Fortune.Repository.Basic;
using Fortune.Repository.DBContext;
using Fortune.Repository.ModelExtension;
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
        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Package)
                .Select(o => new OrderDTO
                {
                    UserName = o.User != null ? o.User.UserName : "guest",
                    FullName = o.User != null ? o.User.FullName : "",
                    Email = o.User != null ? o.User.Email : o.GuestEmail,
                    PackageName = o.Package.Name,
                })
                .ToListAsync();
        }
        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        }
        public async Task<Order?> GetOrdersByOrderCodeAsync(long orderCode)
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
        public async Task<List<string?>> getPackageUserPurchased(Guid id)
        {
            return await _context.Orders
                .Where(o => o.UserId == id && o.Status == OrderStatus.Paid)
                .Select(o => o.Package.Name)
                .ToListAsync();
        }
    }
}
