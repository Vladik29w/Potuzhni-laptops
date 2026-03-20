using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace LaptopServer.DTO
{
    public record UserDTO
    {
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
    public record RegisterDTO
    {
        public required string Name { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
    public record LoginDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
    public record RefreshTokenDTO
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public required string UserId { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsUsed { get; set; }
        public bool IsActive => !IsRevoked && !IsUsed && DateTime.UtcNow < Expires;
    }
    public record UserTokensDTO
    {
        public UserDTO User { get; set; }
        public required string Token {  get; set; }
        public required string RefrehToken { get; set; }

    }

}
