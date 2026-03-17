using LaptopServer.DTO;
using LaptopServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController (IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> UserRegister(RegisterDTO register)
        {
            var (user, token, refresh) = await _accountService.UserRegister(register);
            if (user == null) return Unauthorized();
            SetCookie(token);
            SetRefreshCookie(refresh);
            return Ok(user);
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> UserLogin(LoginDTO login)
        {
            var (user, token, refresh) = await _accountService.UserLogin(login);
            if (user == null) return Unauthorized();
            SetCookie(token);
            SetRefreshCookie(refresh);
            return Ok(user);
        }
        [Authorize]
        [HttpGet("me")]
        public ActionResult<UserDTO> GetActiveUser()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
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
        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh()
        {
            var refToken = Request.Cookies["refToken"];
            if (string.IsNullOrEmpty(refToken)) return Unauthorized();

            var (user, token, refresh) = await _accountService.RefreshUserToken(refToken);
            if (user == null) return Unauthorized();

            SetCookie(token);
            SetRefreshCookie(refresh);
            return Ok(user);
        }
        private void SetCookie (string token)
        {
            var cookie = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(10)
            };
            Response.Cookies.Append("jwt", token, cookie);
        }
        private void SetRefreshCookie(string refToken)
        {
            var cookie = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(1),
            };
            Response.Cookies.Append("refToken", refToken, cookie);
        }
    }
}
