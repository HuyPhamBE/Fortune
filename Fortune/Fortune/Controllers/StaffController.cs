using Fortune.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fortune.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : Controller
    {
        private readonly IStaffService staffService;

        public StaffController(IStaffService staffService)
        {
            this.staffService = staffService;
        }
        [HttpGet("{username}")]
        [Authorize(Roles = "3,1,2")]
        public async Task<IActionResult> GetStaffByUsername(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("Username cannot be null or empty.");
            }
            var staff = await staffService.GetStaffByUsernameAsync(userName);
            if (staff == null)
            {
                return NotFound();
            }
            return Ok(staff);
        }
        [HttpGet]
        [Authorize(Roles = "3,1")]
        public async Task<IActionResult> GetAllStaffAccounts()
        {
            var staffList = await staffService.GetAllStaffAccountsAsync();
            if (staffList == null || !staffList.Any())
            {
                return NotFound("No staff accounts found.");
            }
            return Ok(staffList);
        }
    }
}