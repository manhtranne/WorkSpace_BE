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
            var result = await _accountService.AuthenticateAsync(request, GenerateIPAddress());
            return Ok(result.Data);
        }
        [HttpPost("register")]
        // API Request URL: POST /api/accounts/register
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            var origin = GetOrigin();
            var result = await _accountService.RegisterAsync(request, origin, GenerateIPAddress());
            return Ok(result.Data);
        }
        [HttpGet("confirm-email")]
        // API Request URL: GET /api/accounts/confirm-email
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string code)
        {
            var result = await _accountService.ConfirmEmailAsync(userId, code);
            return Ok(result.Data);
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
            var result = await _accountService.ResetPassword(model);
            return Ok(result.Data);
        }
        
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var result = await _accountService.RefreshTokenAsync(request, GenerateIPAddress());
            return Ok(result.Data);
        }
        
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] string token)
        {
            var result = await _accountService.RevokeTokenAsync(token, GenerateIPAddress());
            return Ok(result.Data);
        }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            var result = await _accountService.RevokeTokenAsync(refreshToken, GenerateIPAddress());
            return Ok(result.Data);
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