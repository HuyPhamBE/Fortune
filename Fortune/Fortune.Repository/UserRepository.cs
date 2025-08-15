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
    public class UserRepository:GenericRepository<User>
    {
        public UserRepository(FortuneContext context) : base(context)
        {
        }
        public async Task<User> GetUserByUsernameAsync(string userName)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        }
        public async Task<List<User>> GetAllUserAccounts()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<bool> IsUserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}
