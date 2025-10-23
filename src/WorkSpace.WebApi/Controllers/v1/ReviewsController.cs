using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Reviews; 
using WorkSpace.Application.Features.Reviews.Commands.CreateReview;
using WorkSpace.Application.Wrappers;
using TravelBooking.Extensions; 

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/reviews")]
    [ApiController]
    [Authorize] 
    public class ReviewsController : BaseApiController
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto request)
        {
            var userId = User.GetUserId();

            if (userId == 0)
            {
                return Unauthorized(new Response<string>("User not authenticated."));
            }

            var command = new CreateReviewCommand
            {
                Dto = request,
                UserId = userId
            };

            var result = await Mediator.Send(command);
            return Ok(result);
        }
    }
}