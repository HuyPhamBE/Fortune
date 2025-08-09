using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Fortune.Repository;
using Fortune.Repository.Basic;
using Fortune.Repository.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        Task<Mini_game> UploadMiniGameAsync(IFormFile file, string mgName, string mgDes, string publicId);
    }
    public class MiniGameService : IMiniGameService
    {
        private readonly MiniGameRepository miniGameRepository;
        private readonly Cloudinary cloudinary;

        public MiniGameService(MiniGameRepository miniGameRepository, Cloudinary cloudinary)
        {
            this.miniGameRepository = miniGameRepository;
            this.cloudinary = cloudinary;
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
        public async Task<Mini_game> UploadMiniGameAsync(IFormFile file,
                                                        string mgName,
                                                        string mgDes,
                                                        string publicId)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("File is empty");
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                PublicId = publicId,
                Overwrite = true
            };
            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Cloudinary upload failed");
            var miniGame = new Mini_game
            {
                miniGame_id = Guid.NewGuid(),
                MG_name = mgName,
                MG_des = mgDes,
                FileName = file.FileName,
                FileType = file.ContentType,
                FileUrl = uploadResult.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.PublicId
            };
            await miniGameRepository.CreateAsync(miniGame);
            return miniGame;
        }
        public async Task<bool> DeleteMiniGameAsync(Guid id)
        {
            var miniGame = await miniGameRepository.GetMiniGameByIdAsync(id);
            if (miniGame == null) return false;

            // Delete file from Cloudinary first
            if (!string.IsNullOrEmpty(miniGame.PublicId))
            {
                var deleteParams = new DeletionParams(miniGame.PublicId)
                {
                    ResourceType = ResourceType.Raw // since we're uploading as raw files
                };
                var deletionResult = await cloudinary.DestroyAsync(deleteParams);
                if (deletionResult.StatusCode != System.Net.HttpStatusCode.OK &&
                    deletionResult.Result != "ok")
                {
                    throw new Exception("Failed to delete file from Cloudinary");
                }
            }
            // Now delete the mini game record from the database
            await miniGameRepository.RemoveAsync(miniGame);
            return true;
        }

    }
}
