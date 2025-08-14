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
    public class PackageRepository : GenericRepository<Package>
    {
        public PackageRepository(FortuneContext context) : base(context)
        {
        }
        public async Task<Package> GetPackageByIdAsync(Guid id)
        {
            return await _context.Packages.FirstOrDefaultAsync(p => p.package_Id == id);
        }
        public async Task<List<Package>> GetAllPackageAsync()
        {
            return await _context.Packages.ToListAsync();
        }
    }
}
