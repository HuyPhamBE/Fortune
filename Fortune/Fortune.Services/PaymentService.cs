using Fortune.Repository;
using Fortune.Repository.Models;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Services
{
    public interface IPaymentService
    {
        Task<(string checkoutUrl, long orderCode)> CreateOrderAsync(Guid packageId, string userId, string guestEmail);
        Task<bool> VerifyWebhook(WebhookType payload);
        Task<int> ClaimOrdersForUserAsync(Guid userId, string email);
    }
    public class PaymentService : IPaymentService
    {
        private readonly PayOS payOS;
        private readonly PackageRepository packageRepository;
        private readonly OrderRepository orderRepository;

        public PaymentService(PayOS payOS,
                PackageRepository packageRepository,
                OrderRepository orderRepository)
        {
            this.payOS = payOS;
            this.packageRepository = packageRepository;
            this.orderRepository = orderRepository;
        }


        async Task<(string checkoutUrl, long orderCode)> IPaymentService.CreateOrderAsync(Guid packageId, string userId, string guestEmail)
        {
            var package = await packageRepository.GetPackageByIdAsync(packageId);
            if (package == null) throw new Exception("Package not found");

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var item = new ItemData(package.description, 1, package.price);
            var paymentData = new PaymentData(
                    orderCode: orderCode,
                    amount: package.price,
                    description: $"FortunePayment",
                    items: new List<ItemData> { item },
                    cancelUrl: "https://fortune-fpt.vercel.app/payment/cancel",
                    returnUrl: "https://fortune-fpt.vercel.app/payment/success");

            var created = await payOS.createPaymentLink(paymentData);

            var newOrder = new Order
            {
                OrderCode = orderCode,
                PackageId = package.package_Id,
                CheckoutUrl = created.checkoutUrl,
                PaymentLinkId = created.paymentLinkId,
                Status = OrderStatus.Pending,
                Amount = package.price,
                UserId = string.IsNullOrEmpty(userId) ? (Guid?)null : Guid.Parse(userId),
                GuestEmail = guestEmail,
                Description= $"Fortune payment for purchase {package.package_Id} for user {userId}"
            };
            await orderRepository.CreateAsync(newOrder);
            return (created.checkoutUrl, orderCode);
        }

        async Task<bool> IPaymentService.VerifyWebhook(WebhookType payload)
        {
            var data = payOS.verifyPaymentWebhookData(payload);
            if (data != null && data.code == "00" && data.desc == "Thành công") 
            {
                var order = await orderRepository.GetOrdersByOrderCodeAsync(data.orderCode);
                if (order != null && order.Status != OrderStatus.Paid)
                {
                    order.Status = OrderStatus.Paid;
                    order.PaidAt = DateTime.UtcNow;
                    await orderRepository.UpdateAsync(order);
                }
                return true;
            }
            return false;
        }
        public async Task<int> ClaimOrdersForUserAsync(Guid userId, string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            var unclaimedOrders = await orderRepository.GetUnclaimedOrdersByEmailAsync(email);
            if (!unclaimedOrders.Any())
                return 0;
            foreach (var order in unclaimedOrders)
            {
                order.UserId = userId;
                order.GuestEmail = null; // clear guest link
            }
            await orderRepository.SaveAsync();
            return unclaimedOrders.Count;

        }  
    }
}
