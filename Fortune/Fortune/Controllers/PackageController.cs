using Fortune.DTOs;
using Fortune.Repository.Models;
using Fortune.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fortune.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService packageService;

        public PackageController(IPackageService packageService)
        {
            this.packageService = packageService;
        }
        [HttpGet]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> GetAllPackages()
        {
            var packages = await packageService.GetAllPackageAsync();
            return Ok(packages);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> GetPackageById(Guid id)
        {
            var package = await packageService.GetPackageByIdAsync(id);
            if (package == null)
            {
                return NotFound();
            }
            return Ok(package);
        }
        [HttpPost]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> CreatePackage([FromBody] PackageDTO package)
        {
            if (package == null)
            {
                return BadRequest("Package cannot be null.");
            }
            // Map PackageDTO to Package model
            var packageModel = new Package
            {
                package_Id = Guid.NewGuid(),
                description = package.description,
                price = package.price,
            };
            var result = await packageService.CreatePackageAsync(packageModel);
            return CreatedAtAction(nameof(GetPackageById), new { id = packageModel.package_Id }, result);
        }
    }
}
