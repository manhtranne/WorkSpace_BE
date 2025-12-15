using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WorkSpace.Application.Features.Notifications;
using WorkSpace.Application.Features.Notifications.Queries;
using WorkSpace.Application.Features.Notifications_F.Commands;

namespace WorkSpace.WebApi.Controllers.v1
{
    public class NotificationController : BaseApiController
    {
        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdminNotification([FromBody] CreateAdminNotificationCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("owner")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> CreateOwnerNotification([FromBody] CreateOwnerNotificationCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpGet("system")]
        public async Task<IActionResult> GetSystemNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return Ok(await Mediator.Send(new GetSystemNotificationsQuery { PageNumber = pageNumber, PageSize = pageSize }));
        }

        [HttpGet("personal")]
        [Authorize]
        public async Task<IActionResult> GetPersonalNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return Ok(await Mediator.Send(new GetPersonalNotificationsQuery { PageNumber = pageNumber, PageSize = pageSize }));
        }
    }
}