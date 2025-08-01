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
    public class StaffRepository:GenericRepository<Staff>
    {
        public StaffRepository(FortuneContext context) : base(context)
        {
        }
        private readonly UserRepository _userRepository;
        public async Task<Staff> GetStaffByUsernameAsync(string userName)
        {
            var staffName =await _userRepository.GetUserByUsernameAsync(userName);
            return await _context.Staff.FirstOrDefaultAsync(s => staffName.UserName == userName);
        }
        public async Task<List<Staff>> GetAllStaffAccountsAsync()
        {
            return await _context.Staff.ToListAsync();
        }
        public async Task<Staff> GetStaffByIdAsync(Guid staffId)
        {
            return await _context.Staff.FindAsync(staffId);
        }
    }
}
