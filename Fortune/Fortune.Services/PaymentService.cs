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
        Task<(bool success, string reason)> VerifyWebhook(WebhookType payload);

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

        public async Task<(bool success, string reason)> VerifyWebhook(WebhookType payload)
        {
            try
            {
                Console.WriteLine("=== WEBHOOK VERIFICATION START ===");

                // First verify the webhook signature and get the data
                Console.WriteLine("Step 1: Verifying PayOS signature...");
                var data = payOS.verifyPaymentWebhookData(payload);
                if (data == null)
                {
                    Console.WriteLine("ERROR: PayOS signature verification failed");
                    return (false, "Signature verification failed or payload invalid");
                }
                Console.WriteLine("✅ PayOS signature verified successfully");

                Console.WriteLine($"Step 2: Checking payment data - Code={data.code}, Desc='{data.desc}', OrderCode={data.orderCode}");

                if (data.code != "00")
                {
                    Console.WriteLine($"ERROR: Unexpected payment code: {data.code}");
                    return (false, $"Unexpected code: {data.code}");
                }
                Console.WriteLine("✅ Payment code is valid (00)");

                // Check for "success" in English (as seen in your logs)
                if (!data.desc.Equals("success", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"ERROR: Unexpected description: '{data.desc}' (expected 'success')");
                    return (false, $"Unexpected description: {data.desc}");
                }
                Console.WriteLine("✅ Payment description is valid (success)");

                Console.WriteLine($"Step 3: Looking up order in database with orderCode: {data.orderCode} (Type: {data.orderCode.GetType()})");
                Console.WriteLine($"OrderCode value: {data.orderCode}");
                Console.WriteLine($"OrderCode as string: '{data.orderCode.ToString()}'");

                // Add detailed database lookup logging
                try
                {
                    Console.WriteLine("Calling orderRepository.GetOrdersByOrderCodeAsync...");
                    var order = await orderRepository.GetOrdersByOrderCodeAsync(data.orderCode);
                    Console.WriteLine("Repository call completed");

                    if (order == null)
                    {
                        Console.WriteLine($"❌ ORDER NOT FOUND in database for orderCode: {data.orderCode}");

                        // Let's try to debug this further
                        Console.WriteLine("Attempting to list some orders for debugging...");
                        try
                        {
                            // You might need to add this method to your repository or use a different approach
                            // This is just for debugging - remove in production
                            Console.WriteLine("Debug: Trying to find any orders with similar orderCodes...");
                        }
                        catch (Exception debugEx)
                        {
                            Console.WriteLine($"Debug query failed: {debugEx.Message}");
                        }

                        return (false, $"Order not found for orderCode {data.orderCode}");
                    }

                    Console.WriteLine($"✅ ORDER FOUND: ID={order.Id}, Status={order.Status}, Amount={order.Amount}, UserId={order.UserId}");

                    if (order.Status == OrderStatus.Paid)
                    {
                        Console.WriteLine("⚠️ Order already marked as paid - no update needed");
                        return (true, "Order already marked as paid - webhook processed successfully");
                    }

                    Console.WriteLine("Step 4: Updating order status to Paid...");
                    order.Status = OrderStatus.Paid;
                    order.PaidAt = DateTime.UtcNow;

                    await orderRepository.UpdateAsync(order);
                    Console.WriteLine("✅ Order successfully updated to Paid status");

                    Console.WriteLine("=== WEBHOOK VERIFICATION SUCCESS ===");
                    return (true, "Order updated to Paid");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"❌ DATABASE ERROR during order lookup: {dbEx.Message}");
                    Console.WriteLine($"Database StackTrace: {dbEx.StackTrace}");
                    throw; // This will be caught by the outer try-catch
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPTION in VerifyWebhook: {ex.Message}");
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw; // Re-throw to be caught by controller
            }
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
