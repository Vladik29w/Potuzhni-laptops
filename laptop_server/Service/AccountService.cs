using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Common;

namespace LaptopServer.Service
{
    public interface IAccountService
    {
        Task<ErrorOr<UserTokensDTO>> UserRegister(RegisterDTO register);
        Task<ErrorOr<UserTokensDTO>> UserLogin(LoginDTO login);
        Task<ErrorOr<UserTokensDTO>> RefreshUserToken(string token);
        Task<ErrorOr<Success>> UserLogout(string token);
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

        public async Task<ErrorOr<UserTokensDTO>> UserRegister(RegisterDTO register)
        {
            var user = new IdentityUser
            {
                UserName = register.Name,
                Email = register.Email,
            };
            var res = await _manager.CreateAsync(user, register.Password);
            if (!res.Succeeded)
            {
                var errors = res.Errors
                    .Select(e => Error.Validation(code: e.Code, description: e.Description))
                    .ToList();
                return errors;
            }
            var roles = new List<string> { "User" };
            await _manager.AddToRoleAsync(user, roles[0]);
            var token = await _tokenService.GetTokenAsync(user);

            var refreshToken = await SetRefreshToken(user.Id);
            return new UserTokensDTO
            {
                User = new UserDTO { Email = register.Email!, Roles = roles },
                Token = token,
                RefrehToken = refreshToken,
            };
        }
        public async Task<ErrorOr<UserTokensDTO>> UserLogin(LoginDTO login)
        {
            var user = await _manager.FindByEmailAsync(login.Email);
            if (user == null)
                return Error.NotFound(code: "UserNotFound");
            var passwordValid = await _manager.CheckPasswordAsync(user, login.Password);
            if (!passwordValid)
                return Error.Validation(code: "InvalidPassword");
            var roles = await _manager.GetRolesAsync(user);
            var token = await _tokenService.GetTokenAsync(user);
            var refreshToken = await SetRefreshToken(user.Id);
            return new UserTokensDTO
            {
                User = new UserDTO { Email = login.Email!, Roles = roles.ToList() },
                Token = token,
                RefrehToken = refreshToken,
            };
        }
        public async Task<ErrorOr<Success>> UserLogout(string refToken)
        {
            var activeToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refToken);
            if (activeToken == null)
                return Error.NotFound(code: "TokenNotFound");
            _dbContext.RefreshTokens.Remove(activeToken);
                await _dbContext.SaveChangesAsync();
            return Result.Success;
        }
        public async Task<ErrorOr<UserTokensDTO>> RefreshUserToken(string token)
        {
            var curToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && !t.IsRevoked);
            if (curToken == null || curToken.Expires < DateTime.UtcNow)
                return Error.Unauthorized(code: "InvalidToken");
            var user = await _manager.FindByIdAsync(curToken.UserId);
            if (user == null)
                return Error.NotFound(code: "UserNotFound");
            curToken.IsUsed = true;
            _dbContext.RefreshTokens.Update(curToken);
            await _dbContext.SaveChangesAsync();
            var roles = await _manager.GetRolesAsync(user);
            var newJwt = await _tokenService.GetTokenAsync(user);
            var newRefresh = await SetRefreshToken(user.Id);

            var userDto = new UserDTO { Email = user.Email!, Roles = roles.ToList() };
            return new UserTokensDTO
            {
                User = userDto,
                Token = newJwt,
                RefrehToken = newRefresh,
            };
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
