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
        Task<Plan> UploadPlanAsync(IFormFile file, string planName, string planDes);
    }
    public class PlanService : IPlanService
    {
        private readonly PlanRepository planRepository;

        public PlanService(PlanRepository planRepository)
        {
            this.planRepository = planRepository;
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
        public async Task<Plan> UploadPlanAsync(IFormFile file, string planName, string planDes)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var plan = new Plan
            {
                Plan_id = Guid.NewGuid(),
                Plan_name = planName,
                Plan_des = planDes,
                FileName = file.FileName,
                FileType = file.ContentType,
                FileData = memoryStream.ToArray(),
            };

            await planRepository.CreateAsync(plan);
            await planRepository.SaveAsync();
            return plan;
        }
    }
}
