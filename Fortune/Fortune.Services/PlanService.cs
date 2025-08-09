using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Fortune.Repository;
using Fortune.Repository.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Services
{
    public interface IPlanService
    {
        Task<Plan> GetPlanByIdAsync(Guid planId);
        Task<List<Plan>> GetAllPlansAsync();
        Task<int> AddPlanAsync(Plan plan);
        Task<int> UpdatePlanAsync(Plan plan);
        Task<Plan> UploadPlanAsync(IFormFile file, string planName, string planDes, string publicId);
    }
    public class PlanService : IPlanService
    {
        private readonly PlanRepository planRepository;
        private readonly Cloudinary cloudinary;

        public PlanService(PlanRepository planRepository, Cloudinary cloudinary)
        {
            this.planRepository = planRepository;
            this.cloudinary = cloudinary;
        }
        public async Task<Plan> GetPlanByIdAsync(Guid planId)
        {
            return await planRepository.GetPlanByIdAsync(planId);
        }
        public async Task<List<Plan>> GetAllPlansAsync()
        {
            return await planRepository.GetAllPlansAsync();
        }
        public async Task<int> AddPlanAsync(Plan plan)
        {
            return await planRepository.CreateAsync(plan);

        }
        public async Task<int> UpdatePlanAsync(Plan plan)
        {
            return await planRepository.UpdateAsync(plan);

        }
        public async Task<Plan> UploadPlanAsync(IFormFile file, string planName, string planDes, string publicId)
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

            var plan = new Plan
            {
                Plan_id = Guid.NewGuid(),
                Plan_name = planName,
                Plan_des = planDes,
                FileName = file.FileName,
                FileType = file.ContentType,
                FileUrl = uploadResult.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.PublicId
            };

            await planRepository.CreateAsync(plan);
            return plan;
        }

        public async Task<bool> DeletePlanAsync(Guid id)
        {
            var plan = await planRepository.GetPlanByIdAsync(id);
            if (plan == null) return false;

            // Delete file from Cloudinary first
            if (!string.IsNullOrEmpty(plan.PublicId))
            {
                var deleteParams = new DeletionParams(plan.PublicId)
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
            await planRepository.RemoveAsync(plan);
            return true;
        }
    }
}
