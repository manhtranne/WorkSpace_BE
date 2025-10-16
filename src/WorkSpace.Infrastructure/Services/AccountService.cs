﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<AccountService> _logger;
        
        public AccountService(UserManager<AppUser> userManager, 
            RoleManager<AppRole> roleManager, 
            IOptions<JWTSettings> jwtSettings, 
            IDateTimeService dateTimeService, 
            SignInManager<AppUser> signInManager,
            IEmailService emailService,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _dateTimeService = dateTimeService;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
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
                
                // Save refresh token to database
                user.RefreshToken = refreshToken.Token;
                user.RefreshTokenExpiryTime = refreshToken.Expires;
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                
                _logger.LogInformation("Authentication successful for user {UserId} ({Email}) from IP {IpAddress}", user.Id, user.Email, ipAddress);
                
                return new Response<AuthenticationResponse>(response, $"Authenticated {user.UserName}");
            }
            catch (ApiException)
            {
                throw; // Re-throw ApiException as-is
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during authentication for email {Email} from IP {IpAddress}", request.Email, ipAddress);
                throw new ApiException("An error occurred during authentication. Please try again.");
            }
        }

        public async Task<Response<string>> RegisterAsync(RegisterRequest request, string origin)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUserName != null)
            {
                throw new ApiException($"Username '{request.UserName}' is already taken.");
            }
            var user = new AppUser()
            {
                Email = request.Email,
                UserName = request.UserName,
                DateCreated = DateTime.UtcNow,
                IsActive = true,
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Roles.Basic.ToString());
                    var verificationUri = await SendVerificationEmail(user, origin);
                    //TODO: Attach Email Service here and configure it via appsettings
                    await _emailService.SendAsync(new Application.DTOs.Email.EmailRequest() { From = "mail@codewithmukesh.com", To = user.Email, Body = $"Please confirm your account by visiting this URL {verificationUri}", Subject = "Confirm Registration" });
                    return new Response<string>(user.Id.ToString(), message: $"User Registered. Please confirm your account by visiting this URL {verificationUri}");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("User registration failed for email {Email}: {Errors}", request.Email, errors);
                    throw new ApiException($"Registration failed: {errors}");
                }
            }
            else
            {
                throw new ApiException($"Email {request.Email } is already registered.");
            }
        }

        private async Task<JwtSecurityToken> GenerateJWToken(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
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
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
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
            //Email Service Call Here
            return verificationUri;
        }

        public async Task<Response<string>> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if(result.Succeeded)
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
            var account = await _userManager.FindByEmailAsync(model.Email);

            // always return ok response to prevent email enumeration
            if (account == null) return;

            var code = await _userManager.GeneratePasswordResetTokenAsync(account);
            var route = "api/account/reset-password/";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            var emailRequest = new EmailRequest()
            {
                Body = $"You reset token is - {code}",
                To = model.Email,
                Subject = "Reset Password",
            };
            await _emailService.SendAsync(emailRequest);
        }

        public async Task<Response<string>> ResetPassword(ResetPasswordRequest model)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);
            if (account == null) throw new ApiException($"No Accounts Registered with {model.Email}.");
            var result = await _userManager.ResetPasswordAsync(account, model.Token, model.Password);
            if(result.Succeeded)
            {
                return new Response<string>(model.Email, message: $"Password Resetted.");
            }
            else
            {
                throw new ApiException($"Error occured while reseting the password.");
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

                var refreshToken = new RefreshToken
                {
                    Token = request.RefreshToken,
                    Expires = user.RefreshTokenExpiryTime ?? DateTime.UtcNow.AddDays(-1),
                    Created = DateTime.UtcNow.AddDays(-7), // Assume created 7 days ago
                    CreatedByIp = ipAddress
                };

                if (!refreshToken.IsActive)
                {
                    _logger.LogWarning("Refresh token failed: Token expired for user {UserId} from IP {IpAddress}", user.Id, ipAddress);
                    throw new ApiException("Refresh token has expired");
                }

                // Generate new JWT token
                var jwtToken = await GenerateJWToken(user);
                var newRefreshToken = GenerateRefreshToken(ipAddress);

                // Revoke old refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);

                // Set new refresh token
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
                throw; // Re-throw ApiException as-is
            }
            catch (Exception ex)
            {
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
                throw; // Re-throw ApiException as-is
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token revocation from IP {IpAddress}", ipAddress);
                throw new ApiException("An error occurred during token revocation. Please try again.");
            }
        }
    }