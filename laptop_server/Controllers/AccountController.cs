using LaptopServer.DTO;
using LaptopServer.Infrastructure;
using LaptopServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ErrorOr;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> UserRegister(RegisterDTO register)
        {
            var userTokensDTO = await _accountService.UserRegister(register);
            if (userTokensDTO.IsError)
                return Unauthorized(userTokensDTO.FirstError.Description);
            return AuthLogic(userTokensDTO.Value);
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> UserLogin(LoginDTO login)
        {
            var userTokensDTO = await _accountService.UserLogin(login);
            if (userTokensDTO.IsError)
                return Unauthorized(userTokensDTO.FirstError.Description);

            return AuthLogic(userTokensDTO.Value);
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            var refToken = Request.Cookies["refToken"];
            if (refToken == null) return BadRequest();
            await _accountService.UserLogout(refToken);
            Response.ClearCookies();
            return Ok();
        }
        [HttpPost("refresh")]
        public async Task<ActionResult<UserDTO>> Refresh()
        {
            var refToken = Request.Cookies["refToken"];
            if (string.IsNullOrEmpty(refToken)) return BadRequest();
            var userTokensDTO = await _accountService.RefreshUserToken(refToken);
            return AuthLogic(userTokensDTO.Value);
        }
        [Authorize]
        [HttpGet("me")]
        public ActionResult<UserDTO> GetActiveUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(JwtRegisteredClaimNames.Email);
            if (string.IsNullOrEmpty(email)) return BadRequest();
            var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
            return Ok(new UserDTO
            {
                Email = email,
                Roles = roles
            });
        }
        private ActionResult<UserDTO> AuthLogic(UserTokensDTO user)
        {
            if (user.User == null)
                return Unauthorized();
            Response.SetCookie(user.Token);
            Response.SetRefreshCookie(user.RefrehToken);
            return Ok(user.User);
        }
    }
}
