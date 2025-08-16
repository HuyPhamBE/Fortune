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
            var logger = HttpContext.RequestServices.GetService<ILogger<PaymentController>>();

            try
            {
                // Log raw payload as JSON
                var json = System.Text.Json.JsonSerializer.Serialize(rawPayload);
                logger?.LogInformation("Raw webhook payload: {Payload}", json);

                // Convert to WebhookType
                var webhookPayload = System.Text.Json.JsonSerializer.Deserialize<Net.payOS.Types.WebhookType>(json);

                var (success, reason) = await paymentService.VerifyWebhook(webhookPayload);

                if (success)
                {
                    return Ok(new { message = "Webhook verified successfully" });
                }
                else
                {
                    return BadRequest(new { message = $"Webhook verification failed: {reason}" });
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Exception in webhook processing");
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
