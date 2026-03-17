using LaptopServer.DTO;
using Microsoft.AspNetCore.Identity;

namespace LaptopServer.Service
{
    public interface IAccountService
    {
        Task<(UserDTO, string)> UserRegister(RegisterDTO register);
        Task<(UserDTO, string)> UserLogin(LoginDTO login);
    }
    public class AccountService : IAccountService
    {
        readonly UserManager<IdentityUser> _manager;
        readonly ITokenService _tokenService;

        public AccountService(UserManager<IdentityUser> manager, ITokenService tokenService)
        {
            _manager = manager;
            _tokenService = tokenService;
        }

        public async Task<(UserDTO, string)> UserRegister(RegisterDTO register)
        {
            var user = new IdentityUser
            {
                UserName = register.Name,
                Email = register.Email,
            };
            var res = await _manager.CreateAsync(user, register.Password);
            if (!res.Succeeded) return (null, null);
            var roles = new List<string> { "User" };
            await _manager.AddToRoleAsync(user, roles[0]);
            var token = await _tokenService.GetTokenAsync(user);

            var userDto = new UserDTO
            {
                Email = register.Email,
                Roles = roles
            };
            return (userDto, token);
        }
        public async Task<(UserDTO, string)> UserLogin(LoginDTO login)
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
                    return (userDto, token);
                }
            }
            return (null, null);
        }
    }
}
