using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Promotions;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.Promotions.Commands.ActivatePromotion;
using WorkSpace.Application.Features.Promotions.Commands.GeneratePromotion;
using WorkSpace.Application.Features.Promotions.Queries;
using WorkSpace.Application.Features.Promotions.Queries.GetAllAdminPromotions; 
using WorkSpace.Application.Features.Promotions.Queries.GetOwnerPromotions;
using WorkSpace.Application.Features.Promotions.Queries.GetPromotionsByWorkspace;
namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/promotions")]
    [ApiController]
    public class PromotionController : BaseApiController
    {
        [HttpGet("active")]
        public async Task<IActionResult> GetActivePromotions(CancellationToken ct)
        {
            var result = await Mediator.Send(new GetActivePromotionsQuery(), ct);
            return Ok(result);
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Staff)}")]
        public async Task<IActionResult> GetAllAdminPromotions()
        {
            var query = new GetAllAdminPromotionsQuery();
            var result = await Mediator.Send(query);

            return Ok(result.Data);
        }

        [HttpGet("owner/all")]
        [Authorize(Roles = nameof(Roles.Owner))]
        public async Task<IActionResult> GetAllOwnerPromotions()
        {
            var userId = User.GetUserId(); 
            var query = new GetOwnerPromotionsQuery { UserId = userId };
            var result = await Mediator.Send(query);

            return Ok(result.Data);
        }
        [HttpGet("workspace/{workSpaceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPromotionsByWorkspace(int workSpaceId)
        {
            var response = await Mediator.Send(new GetPromotionsByWorkspaceQuery { WorkSpaceId = workSpaceId });

            return Ok(response.Data);
        }

        [HttpPost("admin/generate")]
        [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Staff)}")]
        public async Task<IActionResult> AdminGenerateCode([FromBody] GeneratePromotionDto dto)
        {
            var command = new GeneratePromotionCommand
            {
                Dto = dto,
                RequestUserId = User.GetUserId(),
                IsOwnerCode = false
            };

            var result = await Mediator.Send(command);

            if (!result.Succeeded) return BadRequest(new { error = result.Message });
            return Ok(result.Data);
        }

        [HttpPut("admin/activate/{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Staff)}")]
        public async Task<IActionResult> AdminActivateCode(int id)
        {
            var command = new ActivatePromotionCommand
            {
                PromotionId = id,
                RequestUserId = User.GetUserId(),
                IsOwnerAction = false
            };

            var result = await Mediator.Send(command);

            if (!result.Succeeded) return BadRequest(new { error = result.Message });
            return Ok(new { success = true, message = "Activated successfully" });
        }

        [HttpPost("owner/generate")]
        [Authorize(Roles = nameof(Roles.Owner))]
        public async Task<IActionResult> OwnerGenerateCode([FromBody] GeneratePromotionDto dto)
        {
            var command = new GeneratePromotionCommand
            {
                Dto = dto,
                RequestUserId = User.GetUserId(),
                IsOwnerCode = true
            };

            var result = await Mediator.Send(command);

            if (!result.Succeeded) return BadRequest(new { error = result.Message });
            return Ok(result.Data);
        }

        [HttpPut("owner/activate/{id}")]
        [Authorize(Roles = nameof(Roles.Owner))]
        public async Task<IActionResult> OwnerActivateCode(int id)
        {
            var command = new ActivatePromotionCommand
            {
                PromotionId = id,
                RequestUserId = User.GetUserId(),
                IsOwnerAction = true
            };

            var result = await Mediator.Send(command);

            if (!result.Succeeded) return BadRequest(new { error = result.Message });
            return Ok(new { success = true, message = "Activated successfully" });
        }
    }
}