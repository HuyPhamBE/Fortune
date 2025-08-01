using Fortune.Repository.Basic;
using Fortune.Repository.DBContext;
using Fortune.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Repository
{
    public class PlanRepository:GenericRepository<Plan>
    {
        public PlanRepository(FortuneContext context) : base(context)
        {
        }
        public async Task<Plan> GetPlanByIdAsync(Guid planId)
        {
            return await _context.Plans.FindAsync(planId);
        }
        public async Task<List<Plan>> GetAllPlansAsync()
        {
            return await _context.Plans.ToListAsync();
        }
    }
}
