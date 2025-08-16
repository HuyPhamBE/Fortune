using Fortune.Repository;
using Fortune.Repository.Models;
using Fortune.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fortune.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            this.paymentService = paymentService;
        }
        [HttpPost("{id:guid}/checkout")]
        public async Task<IActionResult> Checkout(Guid id, [FromQuery] string? guestEmail)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            try
            {
                var (checkoutUrl, orderCode) = await paymentService.CreateOrderAsync(id, userId, guestEmail);
                return Ok(new { orderCode, checkoutUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] object rawPayload)
        {
            try
            {
                // Convert to WebhookType
                var json = System.Text.Json.JsonSerializer.Serialize(rawPayload);
                var webhookPayload = System.Text.Json.JsonSerializer.Deserialize<Net.payOS.Types.WebhookType>(json);

                var (success, reason) = await paymentService.VerifyWebhook(webhookPayload);

                if (success)
                {
                    return Ok(new { message = "Webhook processed successfully" });
                }
                else
                {
                    return BadRequest(new { message = $"Webhook verification failed: {reason}" });
                }
            }
            catch (Exception ex)
            {
                // Only log critical errors
                var logger = HttpContext.RequestServices.GetService<ILogger<PaymentController>>();
                logger?.LogError("Webhook processing failed: {Message}", ex.Message);

                return BadRequest(new { message = "Webhook processing failed" });
            }
        }

    }
}
