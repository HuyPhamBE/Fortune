using Fortune.Repository;
using Fortune.Repository.Helper;
using Fortune.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Services
{
    public interface IUserService
    {
        
    }
    public class UserService : IUserService
    {
        private readonly UserRepository userRepository;

        public UserService(UserRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        // Implement methods for user management here, e.g., CreateUser, GetUser, UpdateUser, DeleteUser, etc.
        public async Task<User> GetUserAccountAsync(string userName, string password)
        {
            var user = await userRepository.GetUserByUsernameAsync(userName);
            if (user == null)
                return null;

            bool isPasswordValid = PasswordHelper.VerifyPassword(password, user.Password,user.Salt);
            return isPasswordValid ? user : null;
        }
        public async Task<User> CreateUserAsync(User user, string password)
        {
            user.Salt = PasswordHelper.GenerateSalt();
            user.Password = PasswordHelper.HashPassword(password,user.Salt);
            await userRepository.CreateAsync(user);
            return user;
        }
        
        public async Task<List<User>> GetAllUserAccounts()
        {
            return await userRepository.GetAllUserAccounts();
        }
    }
}
