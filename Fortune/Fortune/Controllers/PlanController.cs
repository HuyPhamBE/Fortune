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

        public PlanController(IPlanService planService)
        {
            this.planService = planService;
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
        public async Task<IActionResult> UploadPlan(IFormFile file, string planName,string planDes)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var result = await planService.UploadPlanAsync(file, planName, planDes);
            return Ok(result);
        }
        [HttpGet("download/{id}")]
        [Authorize(Roles = "3,2,1")]
        public async Task<IActionResult> DownloadPlan(Guid id)
        {
            var plan = await planService.GetPlanByIdAsync(id);
            if (plan == null)
            {
                return NotFound();
            }
            var fileStream = new MemoryStream(plan.FileData);
            return File(fileStream, plan.FileType, plan.FileName);
        }
    }
}
