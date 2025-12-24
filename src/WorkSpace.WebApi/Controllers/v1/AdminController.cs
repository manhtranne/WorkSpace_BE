using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Account;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Features.Admin.Queries;
using WorkSpace.Application.Features.Admin.Queries.GetAllBookings;
using WorkSpace.Application.Features.HostProfile.Commands.ApproveHostProfile;
using WorkSpace.Application.Features.HostProfile.Commands.DeleteHostProfile;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/admin")]
[Authorize(Roles = nameof(Roles.Admin))]
[ApiController]
public class AdminController : BaseApiController
{
    private readonly IAccountService _accountService;

    public AdminController(IAccountService accountService)
    {
        _accountService = accountService;
    }


    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        
        var result = await Mediator.Send(new GetAdminDashboardStatsQuery());
        return Ok(result);
    }



    [HttpGet("owners")]
    public async Task<IActionResult> GetAllOwners([FromQuery] string? search = null)
    {
        var request = new GetAllUsersRequestDto
        {
            SearchTerm = search,
            Role = "Owner"
        };
       
        var result = await _accountService.GetAllUsersAsync(request);
        return Ok(result);
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetAllCustomers([FromQuery] string? search = null)
    {
        var request = new GetAllUsersRequestDto
        {
            SearchTerm = search,
            Role = "Customer"
        };
        var result = await _accountService.GetAllUsersAsync(request);
        return Ok(result);
    }

    [HttpGet("staffs")]
    public async Task<IActionResult> GetAllStaff([FromQuery] string? search = null)
    {
        var request = new GetAllUsersRequestDto
        {
            SearchTerm = search,
            Role = "Staff"
        };
        var result = await _accountService.GetAllUsersAsync(request);
        return Ok(result);
    }


    [HttpPut("users/{userId}/role")]
    public async Task<IActionResult> UpdateUserRole([FromRoute] int userId, [FromBody] UpdateUserRoleDto request)
    {
        var result = await _accountService.SetUserRoleAsync(userId, request.Role);
        return Ok(result);
    }
    [HttpPut("{id}/block")]
    public async Task<IActionResult> BlockOwner([FromRoute] int id)
    {
        return await ToggleUserStatus(id);
    }


    [HttpGet("admin/All-bookings")]
     [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllBookings([FromQuery] GetAllBookingsAdminQuery query)
    {
        var result = await Mediator.Send(query);


        return Ok(result.Data);
    }

    private async Task<IActionResult> ToggleUserStatus(int userId)
    {
     
        var userResult = await _accountService.GetUserByIdAsync(userId);

        if (!userResult.Succeeded || userResult.Data == null)
        {
            return NotFound(new { error = "User not found" });
        }

        bool currentStatus = userResult.Data.IsActive;
        bool newStatus = !currentStatus;

        var request = new UpdateUserStatusDto { IsActive = newStatus };
        var updateResult = await _accountService.UpdateUserStatusAsync(userId, request);

        if (!updateResult.Succeeded)
        {
            return BadRequest(new { error = updateResult.Message });
        }

        return Ok(updateResult.Data);
    }
    [HttpPut("approve-owner/{hostProfileId}")]
    public async Task<IActionResult> ApproveOwnerRegistration(
            [FromRoute] int hostProfileId,
            [FromQuery] bool isApproved = true) 
    {
        var command = new ApproveHostProfileCommand
        {
            HostProfileId = hostProfileId,
            IsApproved = isApproved
        };

        var result = await Mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(new { success = true, isVerified = isApproved });
    }

    [HttpDelete("reject-owner/{hostProfileId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectOwnerRegistration([FromRoute] int hostProfileId)
    {
        var command = new DeleteHostProfileCommand(hostProfileId);

        var result = await Mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(new { success = true, message = "Đã từ chối yêu cầu và xóa hồ sơ đăng ký thành công." });
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserById([FromRoute] int userId)
    {
        var result = await _accountService.GetUserByIdAsync(userId);
        return Ok(result.Data); 
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserByAdminDto request)
    {
        var result = await _accountService.CreateUserByAdminAsync(request);
        return Ok(result.Data); 
    }
}