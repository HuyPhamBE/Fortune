using Fortune.Repository;
using Fortune.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Services
{
    public interface IOrderService
    {
        public Task<List<Order>> GetAllOrder();
        public Task<List<string?>> GetUserPurchasePackageAsync(Guid userId);
        public Task<int> updateOrder(Order order);
    }
    public class OrderService : IOrderService
    {
        private readonly OrderRepository orderRepository;

        public OrderService(OrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        public async Task<List<Order>> GetAllOrder()
        {
            return await orderRepository.GetAllOrdersAsync();
        }
        public async Task<List<string?>> GetUserPurchasePackageAsync(Guid userId)
        {
            return await orderRepository.getPackageUserPurchased(userId);
        }
        public async Task<int> updateOrder(Order order)
        {
            var existingOrder = await orderRepository.GetOrderByIdAsync(order.Id);
            if (existingOrder == null)
            {
                throw new Exception("Order not found");
            }
            existingOrder.Status = order.Status;
            existingOrder.UserId = order.UserId;
            return await orderRepository.UpdateAsync(existingOrder);
        }
    }
}
