using Fortune.Repository;
using Fortune.Repository.Models;
using Fortune.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fortune.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService paymentService;
        private readonly ILogger<PaymentController> logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            this.paymentService = paymentService;
            this.logger = logger;
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
        public async Task<IActionResult> Webhook([FromBody] Net.payOS.Types.WebhookType payload)
        {
            try
            {
                logger.LogInformation("Webhook endpoint called");

                if (payload == null)
                {
                    logger.LogWarning("Webhook payload is null");
                    return BadRequest(new { message = "Payload is null" });
                }

                // Log payload structure for debugging
                try
                {
                    var payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload, Newtonsoft.Json.Formatting.Indented);
                    logger.LogInformation($"Webhook payload structure: {payloadJson}");
                }
                catch (Exception logEx)
                {
                    logger.LogWarning($"Could not serialize payload for logging: {logEx.Message}");
                }

                var result = await paymentService.VerifyWebhook(payload);

                if (result)
                {
                    logger.LogInformation("✅ Webhook processed successfully");
                    return Ok(new { message = "Webhook processed successfully" });
                }
                else
                {
                    logger.LogWarning("❌ Webhook processing failed");
                    return BadRequest(new { message = "Webhook processing failed" });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error processing webhook: {ex.Message}");
                return BadRequest(new { message = $"Webhook processing error: {ex.Message}" });
            }
        }
    }
}
