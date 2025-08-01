using Fortune.Repository;
using Fortune.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Services
{
    public interface IStaffService
    {
        Task<Staff> GetStaffByUsernameAsync(string userName);
        Task<List<Staff>> GetAllStaffAccountsAsync();
    }
    public class StaffService : IStaffService
    {
        private readonly StaffRepository staffRepository;

        public StaffService(StaffRepository staffRepository)
        {
            this.staffRepository = staffRepository;
        }
        public async Task<Staff> GetStaffByUsernameAsync(string userName)
        {
            return await staffRepository.GetStaffByUsernameAsync(userName);
        }
        public async Task<List<Staff>> GetAllStaffAccountsAsync()
        {
            return await staffRepository.GetAllStaffAccountsAsync();
        }
    }
}
