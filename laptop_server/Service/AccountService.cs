using LaptopServer.DB;
using LaptopServer.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;

namespace LaptopServer.Service
{
    public interface IAccountService
    {
        Task<(UserDTO, string, string)> UserRegister(RegisterDTO register);
        Task<(UserDTO, string, string)> UserLogin(LoginDTO login);
        Task<(UserDTO, string, string)> RefreshUserToken(string token);
    }
    public class AccountService : IAccountService
    {
        readonly UserManager<IdentityUser> _manager;
        readonly ITokenService _tokenService;
        readonly LaptopsDBContext _dbContext;

        public AccountService(UserManager<IdentityUser> manager, ITokenService tokenService, LaptopsDBContext dbContext)
        {
            _manager = manager;
            _tokenService = tokenService;
            _dbContext = dbContext;
        }

        public async Task<(UserDTO, string, string)> UserRegister(RegisterDTO register)
        {
            var user = new IdentityUser
            {
                UserName = register.Name,
                Email = register.Email,
            };
            var res = await _manager.CreateAsync(user, register.Password);
            if (!res.Succeeded) return (null, null, null);
            var roles = new List<string> { "User" };
            await _manager.AddToRoleAsync(user, roles[0]);
            var token = await _tokenService.GetTokenAsync(user);
            var userDto = new UserDTO
            {
                Email = register.Email,
                Roles = roles
            };
            var refreshToken = await SetRefreshToken(user.Id);
            return (userDto, token, refreshToken);
        }
        public async Task<(UserDTO, string, string)> UserLogin(LoginDTO login)
        {
            var user = await _manager.FindByEmailAsync(login.Email);
            if (user != null)
            {
                var passwordValid = await _manager.CheckPasswordAsync(user, login.Password);
                if (passwordValid)
                {
                    var roles = await _manager.GetRolesAsync(user);
                    var token = await _tokenService.GetTokenAsync(user);
                    var userDto = new UserDTO
                    {
                        Email = login.Email,
                        Roles = roles.ToList()
                    };
                    var refreshToken = await SetRefreshToken(user.Id);
                    return (userDto, token, refreshToken);
                }
            }
            return (null, null, null);
        }
        public async Task<(UserDTO, string, string)> RefreshUserToken(string token)
        {
            var curToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && !t.IsRevoked);
            if (curToken == null || curToken.Expires < DateTime.UtcNow)
                return (null,null,null);
            var user = await _manager.FindByIdAsync(curToken.UserId);
            if (user == null)
                return (null, null, null);
            curToken.IsUsed = true;
            _dbContext.RefreshTokens.Update(curToken);

            var roles = await _manager.GetRolesAsync(user);
            var newJwt = await _tokenService.GetTokenAsync(user);
            var newRefresh = await SetRefreshToken(user.Id);

            var userDto = new UserDTO { Email = user.Email, Roles = roles.ToList() };
            return (userDto, newJwt, newRefresh);
        }
        private async Task<string> SetRefreshToken(string userId)
        {
            var refString = _tokenService.GetRefreshToken();
            var refToken = new Entities.RefreshToken
            {
                Token = refString,
                UserId = userId,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(1),
                IsRevoked = false,
                IsUsed = false,
            };
            await _dbContext.RefreshTokens.AddAsync(refToken);
            await _dbContext.SaveChangesAsync();
            return refString;
        }
    }
}
