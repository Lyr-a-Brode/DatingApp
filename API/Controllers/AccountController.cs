using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTO;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register(RegisterUserRequest request)
        {
            if (await UserExists(request.UserName))
            {
                return BadRequest("User name is taken");
            }
            
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = request.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password)),
                PasswordSalt = hmac.Key
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new UserResponse
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserResponse>> Login(LoginUserRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == request.UserName.ToLower());

            if (user == null) return Unauthorized("Invalid username or password");
            
            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password));
            
            if (!computedHash.SequenceEqual(user.PasswordHash)) return Unauthorized("Invalid username or password");

            return new UserResponse
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }
        

        private async Task<bool> UserExists(string userName)
        {
            return await _context.Users.AnyAsync(user => user.UserName == userName.ToLower());
        }
    }
}