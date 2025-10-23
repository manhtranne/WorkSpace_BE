using MediatR;
using WorkSpace.Application.DTOs.Reviews;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewCommand : IRequest<Response<int>>
    {
        public CreateReviewDto Dto { get; set; }
        public int UserId { get; set; }
    }
}