using Fortune.Repository.Models;
using Fortune.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fortune.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;
        public OrderController(IOrderService orderService)
        {
            this.orderService = orderService;            
        }
        [HttpGet("GetAllOrder")]
        public async Task<IActionResult> GetAllOrder()
        {
            var orders = await orderService.GetAllOrder();            
            return Ok(orders);
        }
        [HttpGet("GetUserPurchasePackage/{userId}")]
        public async Task<IActionResult> GetUserPurchasePackage(Guid userId)
        {
            var packages = await orderService.GetUserPurchasePackageAsync(userId);
            if (packages == null || !packages.Any())
            {
                return NotFound("No packages found for the user.");
            }
            return Ok(packages);
        }
        [HttpPut("UpdateOrder/{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id,[FromBody] Order order)
        {
            if (order == null)
            {
                return BadRequest("Invalid order data.");
            }
            try
            {               
                var result = await orderService.updateOrder(order);
                return Ok(new { message = "Order updated successfully", affectedRows = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("updateOrderContact/{id}")]
        public async Task<IActionResult> UpdateOrderContact(Guid id)
        {
            try
            {
                var existingOrder = await orderService.GetAllOrderById(id);
                if (existingOrder == null)
                {
                    return NotFound("Order not found.");
                }
                existingOrder.contact = true;
                var result = await orderService.updateOrder(existingOrder);
                return Ok(new { message = "Order contact updated successfully", affectedRows = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
