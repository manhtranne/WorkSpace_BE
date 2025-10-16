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
        // API Request URL: POST /api/accounts/login
        public async Task<IActionResult> AuthenticateAsync(AuthenticationRequest request)
        {
            return Ok(await _accountService.AuthenticateAsync(request, GenerateIPAddress()));
        }
        [HttpPost("register")]
        // API Request URL: POST /api/accounts/register
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            var origin = GetOrigin();
            return Ok(await _accountService.RegisterAsync(request, origin));
        }
        [HttpGet("confirm-email")]
        // API Request URL: GET /api/accounts/confirm-email
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string code)
        {
            return Ok(await _accountService.ConfirmEmailAsync(userId, code));
        }
        [HttpPost("forgot-password")]
        // API Request URL: POST /api/accounts/forgot-password
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            var origin = GetOrigin();
            await _accountService.ForgotPassword(model, origin);
            return Ok();
        }
        [HttpPost("reset-password")]
        // API Request URL: POST /api/accounts/reset-password
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
        
        private string GetOrigin()
        {
            // Try to get origin from header
            var origin = Request.Headers["origin"].ToString();
            
            // If no origin header, construct from request
            if (string.IsNullOrEmpty(origin))
            {
                var scheme = Request.Scheme; // http or https
                var host = Request.Host.Value; // localhost:7105
                origin = $"{scheme}://{host}";
            }
            
            return origin;
        }
    }
}