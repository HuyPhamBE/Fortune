using Fortune.Repository;
using Fortune.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Services
{
    public interface IPackageService
    {
        Task<List<Package>> GetAllPackageAsync();
        Task<Package> GetPackageByIdAsync(Guid packageId);
        Task<int> CreatePackageAsync(Package package);
        Task<int> UpdatePackageAsync(Package package);
    }
    public class PackageService : IPackageService
    {
        private readonly PackageRepository packageRepository;
        public PackageService(PackageRepository packageRepository)
        {
            this.packageRepository = packageRepository;
        }

        public async Task<int> CreatePackageAsync(Package package)
        {
            return await packageRepository.CreateAsync(package);
        }

        public async Task<List<Package>> GetAllPackageAsync()
        {
            return await packageRepository.GetAllPackageAsync();
        }

        public async Task<Package> GetPackageByIdAsync(Guid packageId)
        {
            return await packageRepository.GetPackageByIdAsync(packageId);
        }
        public async Task<int> UpdatePackageAsync(Package package)
        {
            return await packageRepository.UpdateAsync(package);
        }
    }
}
