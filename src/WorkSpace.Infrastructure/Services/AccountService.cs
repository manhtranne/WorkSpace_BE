using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorkSpace.Application.DTOs.Account;
using WorkSpace.Application.DTOs.Email;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces; 
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.ConfigOptions;
using WorkSpace.Domain.Entities;
using WorkSpace.Infrastructure.Helpers;

namespace WorkSpace.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly JWTSettings _jwtSettings;
    private readonly GoogleSettings _googleSettings;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<AccountService> _logger;
    private readonly IApplicationDbContext _dbContext;

    public AccountService(UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IOptions<JWTSettings> jwtSettings,
        IOptions<GoogleSettings> googleSettings,
        IDateTimeService dateTimeService,
        SignInManager<AppUser> signInManager,
        IEmailService emailService,
        ILogger<AccountService> logger,
        IApplicationDbContext dbContext) 
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtSettings = jwtSettings.Value;
        _googleSettings = googleSettings.Value;
        _dateTimeService = dateTimeService;
        _signInManager = signInManager;
        _emailService = emailService;
        _logger = logger;
        _dbContext = dbContext; 
    }

    public async Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request, string ipAddress)
    {
        try
        {
            _logger.LogInformation("Authentication attempt for email: {Email} from IP: {IpAddress}", request.Email, ipAddress);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Authentication failed: No account found for email {Email} from IP {IpAddress}", request.Email, ipAddress);
                throw new ApiException($"No Accounts Registered with {request.Email}.");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Authentication failed: Invalid credentials for email {Email} from IP {IpAddress}", request.Email, ipAddress);
                throw new ApiException($"Invalid Credentials for '{request.Email}'.");
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Authentication failed: Email not confirmed for {Email} from IP {IpAddress}", request.Email, ipAddress);
                throw new ApiException($"Account Not Confirmed for '{request.Email}'.");
            }

     
            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed: Account is locked for {Email}", request.Email);
                throw new ApiException($"Account is locked. Please contact administrator.");
            }

            JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
            AuthenticationResponse response = new AuthenticationResponse();
            response.Id = user.Id.ToString();
            response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = user.Email;
            response.UserName = user.UserName;
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;
            var refreshToken = GenerateRefreshToken(ipAddress);
            response.RefreshToken = refreshToken.Token;


            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpiryTime = refreshToken.Expires;
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Authentication successful for user {UserId} ({Email}) from IP {IpAddress}", user.Id, user.Email, ipAddress);

            return new Response<AuthenticationResponse>(response, $"Authenticated {user.UserName}");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for email {Email} from IP {IpAddress}", request.Email, ipAddress);
            throw new ApiException("An error occurred during authentication. Please try again.");
        }
    }


    public async Task<Response<string>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new ApiException("Không tìm thấy người dùng.");
            }

            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.Avatar = request.Avatar ?? user.Avatar;
            user.Dob = request.DateOfBirth ?? user.Dob;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new Response<string>(user.Id.ToString(), "Cập nhật thông tin thành công.");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ApiException($"Lỗi khi cập nhật: {errors}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi update profile user {UserId}", userId);
            throw new ApiException(ex.Message);
        }
    }
    public async Task<Response<string>> RegisterAsync(RegisterRequest request, string origin, string ipAddress)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUserName != null) throw new ApiException($"Username '{request.UserName}' is already taken.");

            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail != null) throw new ApiException($"Email '{request.Email}' is already registered.");

            var user = new AppUser
            {
                Email = request.Email,
                UserName = request.UserName,
                DateCreated = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ApiException($"Registration failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, Roles.Customer.ToString());

           
            var verificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            verificationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(verificationToken));

            var route = "confirm-email";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "userId", user.Id.ToString());
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", verificationToken);

            var emailBody = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                    .content {{ background-color: white; padding: 30px; border-radius: 0 0 5px 5px; text-align: center; }}
                    .btn {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white !important; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 20px; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Xác Thực Tài Khoản</h1>
                    </div>
                    <div class='content'>
                        <p>Xin chào <strong>{request.UserName}</strong>,</p>
                        <p>Cảm ơn bạn đã đăng ký tài khoản tại WorkSpace. Vui lòng nhấn vào nút bên dưới để kích hoạt tài khoản của bạn:</p>
                        <a href='{verificationUri}' class='btn'>Xác Thực Ngay</a>

                    </div>
                    <div class='footer'>
                        <p>&copy; 2025 WorkSpace. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";

            await _emailService.SendAsync(new EmailRequest
            {
                To = user.Email,
                Body = emailBody,
                Subject = "Xác thực tài khoản WorkSpace"
            });

            _logger.LogInformation("User registered. Verification email sent to: {Email}", user.Email);

            return new Response<string>(user.Id.ToString(), "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user {Email}", request.Email);
            throw new ApiException("An error occurred during registration.");
        }
    }

    private async Task<JwtSecurityToken> GenerateJWToken(AppUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        var roleClaims = new List<Claim>();

        for (int i = 0; i < roles.Count; i++)
        {
            roleClaims.Add(new Claim("role", roles[i]));
        }

        string ipAddress = IpHelper.GetIpAddress();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("uid", user.Id.ToString()),
            new Claim("ip", ipAddress)
        }
        .Union(userClaims)
        .Union(roleClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            signingCredentials: signingCredentials);
        return jwtSecurityToken;
    }

    private string RandomTokenString()
    {
        var randomBytes = new byte[40];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        return BitConverter.ToString(randomBytes).Replace("-", "");
    }

    private async Task<string> SendVerificationEmail(AppUser user, string origin)
    {
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var route = "api/account/confirm-email/";
        var _enpointUri = new Uri(string.Concat($"{origin}/", route));
        var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "userId", user.Id.ToString());
        verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);

        return verificationUri;
    }

    public async Task<Response<string>> ConfirmEmailAsync(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);
        if (result.Succeeded)
        {
            return new Response<string>(user.Id.ToString(), message: $"Account Confirmed for {user.Email}. You can now use the /api/Account/authenticate endpoint.");
        }
        else
        {
            throw new ApiException($"An error occured while confirming {user.Email}.");
        }
    }

    private RefreshToken GenerateRefreshToken(string ipAddress)
    {
        return new RefreshToken
        {
            Token = RandomTokenString(),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    public async Task ForgotPassword(ForgotPasswordRequest model, string origin)
    {
        try
        {
            _logger.LogInformation("Forgot password request for email: {Email}", model.Email);

            var account = await _userManager.FindByEmailAsync(model.Email);

            if (account == null) 
            {
                _logger.LogWarning("Forgot password failed: No account found for {Email}", model.Email);
                throw new ApiException($"Không tìm thấy tài khoản với email {model.Email}.");
            }

            // Tạo mật khẩu mới ngẫu nhiên
            var newPassword = GenerateRandomPassword();
            
            // Tạo token để reset password
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(account);
            
            // Reset password với mật khẩu mới
            var result = await _userManager.ResetPasswordAsync(account, resetToken, newPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Password reset failed for {Email}: {Errors}", model.Email, errors);
                throw new ApiException($"Không thể reset mật khẩu: {errors}");
            }

            // Tạo email body với template đẹp
            var emailBody = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            color: #333;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 20px;
                            background-color: #f9f9f9;
                        }}
                        .header {{
                            background-color: #4CAF50;
                            color: white;
                            padding: 20px;
                            text-align: center;
                            border-radius: 5px 5px 0 0;
                        }}
                        .content {{
                            background-color: white;
                            padding: 30px;
                            border-radius: 0 0 5px 5px;
                        }}
                        .password-box {{
                            background-color: #f0f0f0;
                            border: 2px solid #4CAF50;
                            padding: 15px;
                            margin: 20px 0;
                            text-align: center;
                            font-size: 24px;
                            font-weight: bold;
                            letter-spacing: 2px;
                            border-radius: 5px;
                        }}
                        .warning {{
                            color: #ff6b6b;
                            font-size: 14px;
                            margin-top: 20px;
                        }}
                        .footer {{
                            text-align: center;
                            margin-top: 20px;
                            font-size: 12px;
                            color: #777;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Khôi Phục Mật Khẩu</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào <strong>{account.GetFullName()}</strong>,</p>
                            <p>Chúng tôi đã nhận được yêu cầu khôi phục mật khẩu cho tài khoản của bạn.</p>
                            <p>Mật khẩu mới của bạn là:</p>
                            <div class='password-box'>
                                {newPassword}
                            </div>
                            <p>Vui lòng sử dụng mật khẩu này để đăng nhập và <strong>đổi mật khẩu mới</strong> ngay sau khi đăng nhập để đảm bảo an toàn tài khoản.</p>
                            <div class='warning'>
                                <strong>⚠️ Lưu ý:</strong> Nếu bạn không yêu cầu khôi phục mật khẩu, vui lòng liên hệ với chúng tôi ngay lập tức.
                            </div>
                        </div>
                        <div class='footer'>
                            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
                            <p>&copy; 2024 WorkSpace. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var emailRequest = new EmailRequest()
            {
                Body = emailBody,
                To = model.Email,
                Subject = "Khôi Phục Mật Khẩu - WorkSpace",
            };
            
            await _emailService.SendAsync(emailRequest);

            _logger.LogInformation("Password reset successful and email sent to {Email}", model.Email);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during forgot password for {Email}", model.Email);
            throw new ApiException("Có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.");
        }
    }

    /// <summary>
    /// Tạo mật khẩu ngẫu nhiên an toàn
    /// </summary>
    private string GenerateRandomPassword()
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        const string allChars = lowercase + uppercase + digits + special;

        var random = new Random();
        var password = new StringBuilder();

        // Đảm bảo có ít nhất 1 ký tự từ mỗi loại
        password.Append(lowercase[random.Next(lowercase.Length)]);
        password.Append(uppercase[random.Next(uppercase.Length)]);
        password.Append(digits[random.Next(digits.Length)]);
        password.Append(special[random.Next(special.Length)]);

        // Thêm các ký tự ngẫu nhiên cho đủ độ dài 10 ký tự
        for (int i = 4; i < 10; i++)
        {
            password.Append(allChars[random.Next(allChars.Length)]);
        }

        // Xáo trộn các ký tự để tăng tính ngẫu nhiên
        return new string(password.ToString().OrderBy(c => random.Next()).ToArray());
    }

    public async Task<Response<string>> ResetPassword(ResetPasswordRequest model)
    {
        var account = await _userManager.FindByEmailAsync(model.Email);
        if (account == null) throw new ApiException($"No Accounts Registered with {model.Email}.");

        var result = await _userManager.ResetPasswordAsync(account, model.Token, model.Password);
        if (result.Succeeded)
        {
            return new Response<string>(model.Email, message: $"Password Resetted.");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));

            if (result.Errors.Any(e => e.Code.Contains("InvalidToken") || e.Description.Contains("Invalid token")))
            {
                throw new ApiException($"Invalid or expired reset token.");
            }

            if (result.Errors.Any(e => e.Code.Contains("Password")))
            {
                throw new ApiException($"Password validation failed: {errors}");
            }

            throw new ApiException($"Error occurred while resetting the password: {errors}");
        }
    }

    public async Task<Response<string>> ChangePasswordAsync(int userId, ChangePasswordRequest model)
    {
        try
        {
            _logger.LogInformation("Change password request for user ID: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("Change password failed: User {UserId} not found", userId);
                throw new ApiException($"Không tìm thấy người dùng.");
            }

            // Kiểm tra mật khẩu hiện tại
            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                _logger.LogWarning("Change password failed: Invalid current password for user {UserId}", userId);
                throw new ApiException("Mật khẩu hiện tại không đúng.");
            }

            // Kiểm tra mật khẩu mới phải khác với mật khẩu cũ
            if (model.CurrentPassword == model.NewPassword)
            {
                _logger.LogWarning("Change password failed: New password is same as current password for user {UserId}", userId);
                throw new ApiException("Mật khẩu mới phải khác với mật khẩu hiện tại.");
            }

            // Đổi mật khẩu
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password changed successfully for user {UserId} ({Email})", user.Id, user.Email);

                // Gửi email thông báo đổi mật khẩu thành công
                await SendPasswordChangedEmailAsync(user);

                return new Response<string>(user.Email, message: "Đổi mật khẩu thành công.");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Change password failed for user {UserId}: {Errors}", userId, errors);

                if (result.Errors.Any(e => e.Code.Contains("Password")))
                {
                    throw new ApiException($"Mật khẩu không hợp lệ: {errors}");
                }

                throw new ApiException($"Đổi mật khẩu thất bại: {errors}");
            }
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password change for user {UserId}", userId);
            throw new ApiException("Có lỗi xảy ra khi đổi mật khẩu. Vui lòng thử lại.");
        }
    }

    /// <summary>
    /// Gửi email thông báo đã đổi mật khẩu thành công
    /// </summary>
    private async Task SendPasswordChangedEmailAsync(AppUser user)
    {
        try
        {
            var emailBody = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            color: #333;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 20px;
                            background-color: #f9f9f9;
                        }}
                        .header {{
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            color: white;
                            padding: 30px;
                            text-align: center;
                            border-radius: 10px 10px 0 0;
                        }}
                        .header h1 {{
                            margin: 0;
                            font-size: 24px;
                        }}
                        .content {{
                            background-color: white;
                            padding: 30px;
                            border-radius: 0 0 10px 10px;
                            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                        }}
                        .info-box {{
                            background-color: #e3f2fd;
                            padding: 15px;
                            margin: 20px 0;
                            border-radius: 8px;
                            border-left: 4px solid #2196f3;
                        }}
                        .warning {{
                            background-color: #fff3cd;
                            padding: 15px;
                            margin: 20px 0;
                            border-radius: 5px;
                            border-left: 4px solid #ffc107;
                        }}
                        .footer {{
                            text-align: center;
                            margin-top: 30px;
                            padding-top: 20px;
                            border-top: 2px solid #e9ecef;
                            font-size: 14px;
                            color: #6c757d;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔒 Mật Khẩu Đã Được Thay Đổi</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào <strong>{user.GetFullName()}</strong>,</p>
                            <p>Chúng tôi xác nhận rằng mật khẩu của bạn đã được thay đổi thành công.</p>
                            
                            <div class='info-box'>
                                <strong>📅 Thời gian thay đổi:</strong> {DateTime.Now.ToString("HH:mm - dd/MM/yyyy")}<br/>
                                <strong>📧 Tài khoản:</strong> {user.Email}<br/>
                                <strong>👤 Tên người dùng:</strong> {user.UserName}
                            </div>

                            <p>Bạn có thể sử dụng mật khẩu mới để đăng nhập vào hệ thống.</p>

                            <div class='warning'>
                                <strong>⚠️ Lưu ý bảo mật:</strong><br/>
                                Nếu bạn <strong>KHÔNG PHẢI</strong> là người thực hiện thay đổi này, vui lòng liên hệ với chúng tôi ngay lập tức để bảo vệ tài khoản của bạn.
                            </div>

                            <p>Để đảm bảo an toàn tài khoản:</p>
                            <ul>
                                <li>Không chia sẻ mật khẩu với bất kỳ ai</li>
                                <li>Sử dụng mật khẩu mạnh và duy nhất</li>
                                <li>Thay đổi mật khẩu định kỳ</li>
                            </ul>
                        </div>
                        <div class='footer'>
                            <p><strong>WorkSpace Team</strong></p>
                            <p>📧 Email: support@workspace.com | 📞 Hotline: 1900-xxxx</p>
                            <p style='font-size: 12px; color: #999;'>Email này được gửi tự động, vui lòng không trả lời.</p>
                            <p style='font-size: 12px;'>&copy; 2024 WorkSpace. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            var emailRequest = new EmailRequest
            {
                To = user.Email,
                Subject = "🔒 Mật khẩu đã được thay đổi - WorkSpace",
                Body = emailBody
            };

            await _emailService.SendAsync(emailRequest);
        }
        catch (Exception ex)
        {
            // Log lỗi nhưng không throw
            _logger.LogError(ex, "Failed to send password changed email to {Email}", user.Email);
        }
    }
    public async Task<Response<string>> SetUserRoleAsync(int userId, string newRole)
    {
        try
        {
            // 1. Kiểm tra đầu vào
            if (string.IsNullOrWhiteSpace(newRole))
            {
                throw new ApiException("Tên quyền (Role) không được để trống.");
            }

            // 2. Tìm User
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new ApiException($"Không tìm thấy User với ID {userId}.");
            }

            // 3. Kiểm tra Role mới có tồn tại trong hệ thống không
            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                throw new ApiException($"Quyền '{newRole}' không tồn tại trong hệ thống.");
            }

            // 4. Lấy danh sách role hiện tại
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Danh sách các role cần xóa (Loại trừ role mới nếu user đã có, để tránh xóa rồi thêm lại)
            var rolesToRemove = new List<string>();
            foreach (var role in currentRoles)
            {
                // Nếu role hiện tại trùng với role mới thì giữ lại, không xóa
                if (string.Equals(role, newRole, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // QUAN TRỌNG: Kiểm tra lại chắc chắn user có trong role này trước khi đưa vào danh sách xóa
                // Bước này giúp tránh lỗi "User is not in role..."
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    rolesToRemove.Add(role);
                }
            }

            // 5. Thực hiện xóa các role cũ (Xóa 1 lần - Batch delete)
            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    // Nếu lỗi là "UserNotInRole" thì có thể bỏ qua, nhưng vì đã check IsInRoleAsync ở trên nên ít khả năng xảy ra.
                    var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    throw new ApiException($"Lỗi khi xóa các quyền cũ: {errors}");
                }
            }

            // 6. Thêm role mới (nếu chưa có)
            if (!await _userManager.IsInRoleAsync(user, newRole))
            {
                var addResult = await _userManager.AddToRoleAsync(user, newRole);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    throw new ApiException($"Lỗi khi thêm quyền '{newRole}': {errors}");
                }
            }

            return new Response<string>(user.Id.ToString(), $"Cập nhật quyền thành '{newRole}' thành công.");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi hệ thống khi update role cho User {UserId}", userId);
            throw new ApiException($"Lỗi hệ thống: {ex.Message}");
        }
    }
    public async Task<Response<AuthenticationResponse>> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
    {
        try
        {
            _logger.LogInformation("Refresh token attempt from IP: {IpAddress}", ipAddress);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null)
            {
                _logger.LogWarning("Refresh token failed: Invalid token from IP {IpAddress}", ipAddress);
                throw new ApiException("Invalid refresh token");
            }

        
            if (!user.IsActive)
            {
                throw new ApiException("User is locked/inactive.");
            }

            var refreshToken = new RefreshToken
            {
                Token = request.RefreshToken,
                Expires = user.RefreshTokenExpiryTime ?? DateTime.UtcNow.AddDays(-1),
                Created = DateTime.UtcNow.AddDays(-7),
                CreatedByIp = ipAddress
            };

            if (!refreshToken.IsActive)
            {
                _logger.LogWarning("Refresh token failed: Token expired for user {UserId} from IP {IpAddress}", user.Id, ipAddress);
                throw new ApiException("Refresh token has expired");
            }

            var jwtToken = await GenerateJWToken(user);
            var newRefreshToken = GenerateRefreshToken(ipAddress);

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpiryTime = newRefreshToken.Expires;
            await _userManager.UpdateAsync(user);

            var response = new AuthenticationResponse
            {
                Id = user.Id.ToString(),
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                Email = user.Email,
                UserName = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                IsVerified = user.EmailConfirmed,
                RefreshToken = newRefreshToken.Token
            };

            _logger.LogInformation("Refresh token successful for user {UserId} ({Email}) from IP {IpAddress}", user.Id, user.Email, ipAddress);

            return new Response<AuthenticationResponse>(response, "Token refreshed successfully");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {

            var innerMsg = ex.InnerException?.Message;
            _logger.LogError(ex, "Unexpected error during token refresh from IP {IpAddress}", ipAddress);
            throw new ApiException("An error occurred during token refresh. Please try again.");
        }
    }

    public async Task<Response<string>> RevokeTokenAsync(string token, string ipAddress)
    {
        try
        {
            _logger.LogInformation("Token revocation attempt from IP: {IpAddress}", ipAddress);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == token);

            if (user == null)
            {
                _logger.LogWarning("Token revocation failed: Invalid token from IP {IpAddress}", ipAddress);
                throw new ApiException("Invalid refresh token");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Token revoked successfully for user {UserId} ({Email}) from IP {IpAddress}", user.Id, user.Email, ipAddress);

            return new Response<string>("Token revoked successfully");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token revocation from IP {IpAddress}", ipAddress);
            throw new ApiException("An error occurred during token revocation. Please try again.");
        }
    }


    public async Task<List<UserDto>> GetAllUsersAsync(GetAllUsersRequestDto request)
    {
        try
        {
            _logger.LogInformation("Getting all users (No Pagination)");

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.UserName.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(searchTerm))
                );
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            var users = await query
                .OrderByDescending(u => u.DateCreated)
                .ToListAsync();

            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (!string.IsNullOrWhiteSpace(request.Role) && !roles.Contains(request.Role))
                {
                    continue;
                }

                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Avatar = user.Avatar,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    DateCreated = user.DateCreated,
                    Dob = user.Dob,
                    LastLoginDate = user.LastLoginDate,
                    Roles = roles.ToList(),
                    FullName = user.GetFullName()
                });
            }

            return userDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw new ApiException("An error occurred while retrieving users");
        }
    }

    public async Task<Response<UserDto>> GetUserByIdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Getting user by ID: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new ApiException($"User with ID {userId} not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Avatar = user.Avatar,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                DateCreated = user.DateCreated,
                Dob = user.Dob,
                LastLoginDate = user.LastLoginDate,
                Roles = roles.ToList(),
                FullName = user.GetFullName()
            };

            return new Response<UserDto>(userDto, "User retrieved successfully");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            throw new ApiException("An error occurred while retrieving user");
        }
    }

    public async Task<Response<UserDto>> CreateUserByAdminAsync(CreateUserByAdminDto request)
    {
        try
        {
            _logger.LogInformation("Admin creating user with email: {Email}", request.Email);

            var roleExists = await _roleManager.RoleExistsAsync(request.Role);
            if (!roleExists)
            {
                throw new ApiException($"Role '{request.Role}' does not exist");
            }

            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUserName != null)
            {
                throw new ApiException($"Username '{request.UserName}' is already taken");
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail != null)
            {
                throw new ApiException($"Email '{request.Email}' is already registered");
            }

            var user = new AppUser
            {
                Email = request.Email,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Dob = request.Dob,
                DateCreated = DateTime.UtcNow,
                IsActive = request.IsActive,
                EmailConfirmed = request.EmailConfirmed
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User creation failed for email {Email}: {Errors}", request.Email, errors);
                throw new ApiException($"User creation failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, request.Role);

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Avatar = user.Avatar,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                DateCreated = user.DateCreated,
                Dob = user.Dob,
                LastLoginDate = user.LastLoginDate,
                Roles = roles.ToList(),
                FullName = user.GetFullName()
            };

            _logger.LogInformation("User created successfully by admin: {UserId} ({Email})", user.Id, user.Email);

            return new Response<UserDto>(userDto, "User created successfully");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user by admin");
            throw new ApiException("An error occurred while creating user");
        }
    }

    public async Task<Response<UserDto>> UpdateUserStatusAsync(int userId, UpdateUserStatusDto request)
    {
        try
        {
            _logger.LogInformation("Updating user status for user ID: {UserId} to {IsActive}", userId, request.IsActive);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new ApiException($"User with ID {userId} not found");
            }

            user.IsActive = request.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User status update failed for user {UserId}: {Errors}", userId, errors);
                throw new ApiException($"User status update failed: {errors}");
            }

    
            var hostProfile = await _dbContext.HostProfiles
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hostProfile != null)
            {
        
                var workspaces = await _dbContext.Workspaces
                    .Where(w => w.HostId == hostProfile.Id)
                    .ToListAsync();

                if (workspaces.Any())
                {
                    foreach (var ws in workspaces)
                    {

                        ws.IsActive = request.IsActive;
                        ws.LastModifiedUtc = DateTime.UtcNow;
                    }

                    await _dbContext.SaveChangesAsync(default);
                    _logger.LogInformation("Updated {Count} workspaces status to {IsActive} for owner {OwnerId}",
                        workspaces.Count, request.IsActive, userId);
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Avatar = user.Avatar,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                DateCreated = user.DateCreated,
                Dob = user.Dob,
                LastLoginDate = user.LastLoginDate,
                Roles = roles.ToList(),
                FullName = user.GetFullName()
            };

            _logger.LogInformation("User status updated successfully for user {UserId}", userId);

            return new Response<UserDto>(userDto, $"User status updated to {(request.IsActive ? "Active" : "Inactive")}");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status for user ID: {UserId}", userId);
            throw new ApiException("An error occurred while updating user status");
        }
    }

    public async Task<Response<AuthenticationResponse>> GoogleLoginAsync(GoogleLoginRequest request, string ipAddress)
    {
        try
        {
            _logger.LogInformation("Google login attempt from IP: {IpAddress}", ipAddress);

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleSettings.ClientId }
                };

                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid Google ID token");
                throw new ApiException("Invalid Google token");
            }

            if (string.IsNullOrEmpty(payload.Email))
            {
                throw new ApiException("Email not provided by Google");
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                _logger.LogInformation("Creating new user from Google login: {Email}", payload.Email);

                var customerRole = Roles.Customer.ToString();
                if (!await _roleManager.RoleExistsAsync(customerRole))
                {
                    _logger.LogError("Customer role does not exist in database");
                    throw new ApiException($"Role '{customerRole}' does not exist. Please contact administrator.");
                }

                user = new AppUser
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    EmailConfirmed = true,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    Avatar = payload.Picture,
                    DateCreated = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user from Google login: {Errors}", errors);
                    throw new ApiException($"User creation failed: {errors}");
                }

                var roleResult = await _userManager.AddToRoleAsync(user, customerRole);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to add role to Google user {UserId}: {Errors}", user.Id, errors);

                    await _userManager.DeleteAsync(user);
                    throw new ApiException($"Failed to assign customer role: {errors}");
                }

                _logger.LogInformation("New user created from Google login: {UserId} ({Email})", user.Id, user.Email);
            }
            else
            {
                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user attempted Google login: {Email}", user.Email);
                    throw new ApiException("Your account has been deactivated. Please contact support.");
                }

                _logger.LogInformation("Existing user logging in via Google: {UserId} ({Email})", user.Id, user.Email);
            }

            JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
            AuthenticationResponse response = new AuthenticationResponse
            {
                Id = user.Id.ToString(),
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Email = user.Email,
                UserName = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                IsVerified = user.EmailConfirmed
            };

            var refreshToken = GenerateRefreshToken(ipAddress);
            response.RefreshToken = refreshToken.Token;

            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpiryTime = refreshToken.Expires;
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Google login successful for user {UserId} ({Email})", user.Id, user.Email);

            return new Response<AuthenticationResponse>(response, "Google authentication successful");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            throw new ApiException("An error occurred during Google authentication. Please try again.");
        }
    }
}