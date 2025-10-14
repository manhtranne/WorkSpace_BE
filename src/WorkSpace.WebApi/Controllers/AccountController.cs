using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Account;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateAsync(AuthenticationRequest request)
        {
            return Ok(await _accountService.AuthenticateAsync(request, GenerateIPAddress()));
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            var origin = Request.Headers["origin"];
            return Ok(await _accountService.RegisterAsync(request, origin));
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery]string userId, [FromQuery]string code)
        {
            var origin = Request.Headers["origin"];
            return Ok(await _accountService.ConfirmEmailAsync(userId, code));
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            await _accountService.ForgotPassword(model, Request.Headers["origin"]);
            return Ok();
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            
            return Ok(await _accountService.ResetPassword(model));
        }
        
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            return Ok(await _accountService.RefreshTokenAsync(request, GenerateIPAddress()));
        }
        
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] string token)
        {
            return Ok(await _accountService.RevokeTokenAsync(token, GenerateIPAddress()));
        }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            return Ok(await _accountService.RevokeTokenAsync(refreshToken, GenerateIPAddress()));
        }
        private string GenerateIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
