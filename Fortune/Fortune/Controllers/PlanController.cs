using Fortune.Repository.Models;
using Fortune.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fortune.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService planService;
        private readonly IBookingService bookingService;

        public PlanController(IPlanService planService,IBookingService bookingService)
        {
            this.planService = planService;
            this.bookingService = bookingService;
        }
        [HttpGet]
        [Authorize(Roles = "3,2,1")]
        public async Task<IActionResult> GetAllPlans()
        {
            var plans = await planService.GetAllPlansAsync();
            return Ok(plans);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "3,2,1")]
        public async Task<IActionResult> GetPlanById(Guid id)
        {
            var plan = await planService.GetPlanByIdAsync(id);
            if (plan == null)
            {
                return NotFound();
            }
            return Ok(plan);
        }
        [HttpPost]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> CreatePlan([FromBody] Plan plan)
        {
            if (plan == null)
            {
                return BadRequest("Plan cannot be null.");
            }
            var result = await planService.AddPlanAsync(plan);
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetPlanById), new { id = plan.Plan_id }, plan);
            }
            return BadRequest("Failed to create plan.");
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] Plan plan)
        {
            if (plan == null || plan.Plan_id != id)
            {
                return BadRequest("Invalid plan data.");
            }
            var result = await planService.UpdatePlanAsync(plan);
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest("Failed to update plan.");
        }
        [HttpPost("upload")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> UploadPlan(IFormFile file, string planName,string planDes, string publicId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var result = await planService.UploadPlanAsync(file, planName, planDes,publicId);
            return Ok(new
            {
                Success = true,
                PlanId = result.Plan_id,
                FileName = result.FileName,
                Message = "File uploaded successfully",
                DownloadUrl = result.FileUrl // Cloudinary direct link
            });
        }
        [HttpGet("download/{id}")]
        [Authorize(Roles = "3,2,1")]
        public async Task<IActionResult> DownloadPlan(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var plan = await planService.GetPlanByIdAsync(id);
            if (plan == null)
            {
                return NotFound();
            }
            using var httpClient = new HttpClient();
            try { 
            var fileBytes = await httpClient.GetByteArrayAsync(plan.FileUrl);
                var booking = new Booking
                {
                    booking_id = Guid.NewGuid(),
                    description = $"Download plan: {plan.Plan_name}",
                    type = true,
                    status = 1,
                    minigame_id =null,
                    plan_id =plan.Plan_id,
                    user_id = userId
                };
                await bookingService.CreateBookingAsync(booking);
                plan.count++;
                await planService.UpdatePlanAsync(plan);
                return File(fileBytes, plan.FileType ?? "application/octet-stream", plan.FileName);
            }
            catch (HttpRequestException ex)
            {

                Console.WriteLine($"Download failed: {ex.Message}");
            }
            return StatusCode(StatusCodes.Status500InternalServerError, "Download failed. Please try again later.");
        }
    }
}
