using Fortune.Repository.Models;
using Fortune.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlTypes;

namespace Fortune.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MiniGameController : ControllerBase
    {
        private readonly IMiniGameService miniGameService;

        public MiniGameController(IMiniGameService miniGameService)
        {
            this.miniGameService = miniGameService;
        }
        [HttpGet]
        [Authorize(Roles = "3,2,1")]
        public async Task<IActionResult> GetAllMiniGames()
        {
            var miniGames = await miniGameService.GetAllMiniGamesAsync();
            return Ok(miniGames);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "3,2,1")]
        public async Task<IActionResult> GetMiniGameById(Guid id)
        {
            var miniGame = await miniGameService.GetMiniGameAsync(id);
            if (miniGame == null)
            {
                return NotFound();
            }
            return Ok(miniGame);
        }
        [HttpPost]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> CreateMiniGame([FromBody] Mini_game miniGame)
        {
            if (miniGame == null)
            {
                return BadRequest("Mini_game cannot be null.");
            }
            var result = await miniGameService.AddMiniGameAsync(miniGame);
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetMiniGameById), new { id = miniGame.miniGame_id }, miniGame);
            }
            return BadRequest("Failed to create Mini_game.");
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> UpdateMiniGame(Guid id, [FromBody] Mini_game miniGame)
        {
            if (miniGame == null || miniGame.miniGame_id != id)
            {
                return BadRequest("Invalid Mini_game data.");
            }
            var result = await miniGameService.UpdateMiniGameAsync(miniGame);
            if (result > 0)
            {
                return NoContent();
            }
            return NotFound("Mini_game not found.");
        }

        #region upload
        [HttpPost("upload")]
        [Authorize(Roles = "3,2,1")]
        public async Task<IActionResult> UploadFile(IFormFile file, string mgName, string mgDes,string publicId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var result = await miniGameService.UploadMiniGameAsync(file, mgName, mgDes,publicId);

                return Ok(new
                {
                    Success = true,
                    MiniGameId = result.miniGame_id,
                    FileName = result.FileName,
                    Message = "File uploaded successfully",
                    DownloadUrl = result.FileUrl // Cloudinary direct link
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = "Upload failed",
                    Message = ex.Message
                });
            }
        }
        #endregion

        #region download
        [HttpGet("download/{id}")]
        [Authorize(Roles = "3,2,1")]
        public async Task<IActionResult> Download(Guid id)
        {
            var miniGame = await miniGameService.GetMiniGameAsync(id);
            if (miniGame == null || string.IsNullOrEmpty(miniGame.FileUrl))
                return NotFound("File not found.");

            using var httpClient = new HttpClient();
            var fileBytes = await httpClient.GetByteArrayAsync(miniGame.FileUrl);

            return File(fileBytes, miniGame.FileType ?? "application/octet-stream", miniGame.FileName);

        }
        #endregion
    }
}
