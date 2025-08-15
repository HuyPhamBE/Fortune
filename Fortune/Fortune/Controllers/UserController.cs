using Fortune.Repository;
using Fortune.Repository.Helper;
using Fortune.Repository.Models;
using Fortune.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fortune.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UserController : ControllerBase
    {

        private readonly IConfiguration configuration;
        private readonly UserService userService;
        private readonly IPaymentService paymentService;

        public UserController(IConfiguration configuration,
            UserService userService,
            IPaymentService paymentService)
        {
            this.configuration = configuration;
            this.userService = userService;
            this.paymentService = paymentService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Invalid login request.");
            }
            var user =await  userService.GetUserAccountAsync(request.UserName, request.Password);
            if (user == null)
            {
                Console.WriteLine($"Invalid username or password.: {user}");
                return Unauthorized($"Invalid username or password");
            }
            var token = GenerateJSONWebToken(user);
            // Here you can generate a JWT token or session for the user
            // For simplicity, we will just return the user details
            return Ok(token);
        }
        [HttpGet("User")]
        public async Task<IActionResult> GetAll()
        {
            var user = await userService.GetAllUserAccounts();


            if (user == null)
                return Unauthorized();


            return Ok(user);
        }
        private string GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(configuration["Jwt:Issuer"]
                    , configuration["Jwt:Audience"]
                    , new Claim[]
                    {
                new(ClaimTypes.Name, user.UserName),
                //new(ClaimTypes.Email, systemUserAccount.Email),
                new(ClaimTypes.Role, user.Role.ToString()),
                new(ClaimTypes.NameIdentifier, user.user_id.ToString())
                    },
                    expires: DateTime.Now.AddMinutes(120),
                    signingCredentials: credentials
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }
        [HttpPost("register")]
        public async Task<bool> RegisterUserAsync(UserRegisterRequest userRegisterRequest)
        {
            var existingUser = await userService.GetUserAccountAsync(userRegisterRequest.UserName, userRegisterRequest.Password);
            if (existingUser != null)
                return false;

            var user = new User
            {
                UserName = userRegisterRequest.UserName,
                Password = userRegisterRequest.Password,
                Phone = userRegisterRequest.Phone,
                Email = userRegisterRequest.Email,
                FullName = userRegisterRequest.FullName,
                Role = 1, // Assuming 1 is the default role for a user
                IsActive = true // Assuming the user is active by default
            };

            await userService.CreateUserAsync(user, userRegisterRequest.Password);
            await paymentService.ClaimOrdersForUserAsync(user.user_id, user.Email);
            return true;
        }
        [HttpGet("current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("User is not authenticated.");
            }
            var user = await userService.GetUserByNameAsync(userName);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(new
            {
                user.UserName,
                user.Email,
                user.Phone,
                user.FullName,
                user.Role,
            });
        }
        [HttpPost("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest userUpdateRequest)
        {
            if (userUpdateRequest == null || string.IsNullOrEmpty(userUpdateRequest.UserName))
            {
                return BadRequest("Invalid user update request.");
            }
            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var user = await userService.GetUserByNameAsync(userName);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            user.Email = userUpdateRequest.Email;
            user.Phone = userUpdateRequest.Phone;
            user.FullName = userUpdateRequest.FullName;
            
            await userService.UpdateUserAsync(user);
            return Ok("User updated successfully.");
        }
        public sealed record UserLoginRequest(string UserName, string Password);
        public sealed record UserRegisterRequest(string UserName, string Password, string? Phone, string Email,string FullName);
        public sealed record UserUpdateRequest(string UserName, string Email, string? Phone, string FullName);
    }
}
