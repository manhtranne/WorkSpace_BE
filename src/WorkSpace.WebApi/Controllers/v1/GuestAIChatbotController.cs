using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.AIChatbot;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/guest-ai-chatbot")]
[ApiController]
public class GuestAIChatbotController : ControllerBase
{
    private readonly IAIChatbotService _aiChatbotService;
    private readonly IRecommendationService _recommendationService;

    public GuestAIChatbotController(IAIChatbotService aiChatbotService,
        IRecommendationService recommendationService)
    {
        _aiChatbotService = aiChatbotService;
        _recommendationService = recommendationService;
    }

    [HttpPost("chat")]
    [ProducesResponseType(typeof(GuestChatbotResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Chat(
        [FromBody] GuestChatbotRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message cannot be empty" });
        }

        var response = await _aiChatbotService.ProcessGuestMessageAsync(request, cancellationToken);
        
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("trending")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetTrending(
        [FromQuery] int count = 5,
        CancellationToken cancellationToken = default)
    {
        var trendingWorkspaces = await _recommendationService
            .GetTrendingWorkSpacesAsync(count, cancellationToken);
        return Ok(trendingWorkspaces);
    }
}