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
        public async Task<IActionResult> Checkout(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            try
            {
                var (checkoutUrl, orderCode) = await paymentService.CreateOrderAsync(id, userId);
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
                var result = await paymentService.VerifyWebhook(payload);
                if (result)
                {
                    return Ok(new { message = "Webhook verified successfully" });
                }
                return BadRequest(new { message = "Webhook verification failed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
