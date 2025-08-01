using Fortune.Repository;
using Fortune.Repository.Basic;
using Fortune.Repository.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Services
{
    public interface IMiniGameService
    {
        Task<Mini_game?> GetMiniGameAsync(Guid id);
        Task<List<Mini_game>> GetAllMiniGamesAsync();
        Task<int> AddMiniGameAsync(Mini_game miniGame);
        Task<int> UpdateMiniGameAsync(Mini_game miniGame);
        Task<Mini_game> UploadMiniGameAsync(IFormFile file, string mgName, string mgDes);
    }
    public class MiniGameService : IMiniGameService
    {
        private readonly MiniGameRepository miniGameRepository;

        public MiniGameService(MiniGameRepository miniGameRepository)
        {
            this.miniGameRepository = miniGameRepository;
        }
        public async Task<Mini_game?> GetMiniGameAsync(Guid id)
        {
            return await miniGameRepository.GetByIdAsync(id);
        }
        public async Task<List<Mini_game>> GetAllMiniGamesAsync()
        {
            return await miniGameRepository.GetAllMiniGamesAsync();
        }
        public async Task<int> AddMiniGameAsync(Mini_game miniGame)
        {
          return  await miniGameRepository.CreateAsync(miniGame);
          
        }
        public async Task<int> UpdateMiniGameAsync(Mini_game miniGame)
        {
           return await miniGameRepository.UpdateAsync(miniGame);
            
        }
        public async Task<Mini_game> UploadMiniGameAsync(IFormFile file, string mgName, string mgDes)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var miniGame = new Mini_game
            {
                miniGame_id = Guid.NewGuid(),
                MG_name = mgName,
                MG_des = mgDes,
                FileName = file.FileName,
                FileType = file.ContentType,
                FileData = memoryStream.ToArray(),
                count = 0,
            };

            await miniGameRepository.CreateAsync(miniGame);            
            return miniGame;
        }       
    }
}
