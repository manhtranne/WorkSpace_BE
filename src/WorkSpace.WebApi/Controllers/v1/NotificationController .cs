using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WorkSpace.Application.Features.Notifications.Queries;
using WorkSpace.Application.Features.Notifications_F.Commands;
using WorkSpace.Application.Features.Notifications_F.Queries;

namespace WorkSpace.WebApi.Controllers.v1
{
    public class NotificationController : BaseApiController
    {
        [HttpPost("notification/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdminNotification([FromBody] CreateAdminNotificationCommand command)
        {
            var id = await Mediator.Send(command);
            return Ok(new { id = id, message = "Tạo thành công" });
        }

        [HttpPost("notification/owner")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> CreateOwnerNotification([FromBody] CreateOwnerNotificationCommand command)
        {
            var id = await Mediator.Send(command);
            return Ok(new { id = id, message = "Tạo thành công" });
        }

        [HttpGet("notification/All-Notification-system")]
        public async Task<IActionResult> GetSystemNotifications()
        {
            var result = await Mediator.Send(new GetSystemNotificationsQuery());
            return Ok(result);
        }

        [HttpGet("notification/personal")]
        [Authorize]
        public async Task<IActionResult> GetPersonalNotifications()
        {
            var result = await Mediator.Send(new GetPersonalNotificationsQuery());
            return Ok(result);
        }


        [HttpGet("notification/{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            var result = await Mediator.Send(new GetNotificationByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpPut("notification/update/{id}")]
        [Authorize] 
        public async Task<IActionResult> UpdateNotification(int id, [FromBody] UpdateNotificationCommand command)
        {
            if (id != command.Id) return BadRequest(new { message = "ID không khớp" });

            await Mediator.Send(command);
            return Ok(new { success = true, message = "Cập nhật thành công" });
        }

        [HttpDelete("notification/delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            await Mediator.Send(new DeleteNotificationCommand { Id = id });
            return Ok(new { success = true, message = "Xóa thành công" });
        }
    }
}