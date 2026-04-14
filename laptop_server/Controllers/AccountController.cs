using LaptopServer.DTO;
using LaptopServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ErrorOr;
using LaptopServer.Infrastructure.Extensions;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController(IAccountService accountService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> UserRegister(RegisterDTO register, CancellationToken ct)
        {
            var userTokensDTO = await accountService.UserRegister(register, ct);
            if (userTokensDTO.IsError)
                return Unauthorized(userTokensDTO.FirstError.Code);
            return AuthLogic(userTokensDTO.Value);
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> UserLogin(LoginDTO login, CancellationToken ct)
        {
            var userTokensDTO = await accountService.UserLogin(login, ct);
            if (userTokensDTO.IsError)
                return Unauthorized(userTokensDTO.FirstError.Code);

            return AuthLogic(userTokensDTO.Value);
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout(CancellationToken ct)
        {
            var refToken = Request.Cookies["refToken"];
            if (refToken == null) return BadRequest();
            await accountService.UserLogout(refToken, ct);
            Response.ClearCookies();
            return Ok();
        }
        [HttpPost("refresh")]
        public async Task<ActionResult<UserDTO>> Refresh(CancellationToken ct)
        {
            var refToken = Request.Cookies["refToken"];
            if (string.IsNullOrEmpty(refToken)) return BadRequest();
            var userTokensDTO = await accountService.RefreshUserToken(refToken, ct);
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
