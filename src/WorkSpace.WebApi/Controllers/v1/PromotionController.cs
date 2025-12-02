using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Promotions;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.Promotions.Commands.ActivatePromotion;
using WorkSpace.Application.Features.Promotions.Commands.GeneratePromotion;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/promotions")]
    [ApiController]
    public class PromotionController : BaseApiController
    {

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
            return Ok(result);
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
            return Ok(result);
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
            return Ok(result);
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
            return Ok(result);
        }
    }
}