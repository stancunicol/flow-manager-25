using FlowManager.Application.Interfaces;
using FlowManager.Shared.DTOs.Requests.FormReview;
using Microsoft.AspNetCore.Mvc;

namespace FlowManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormReviewsController : ControllerBase
    {
        private readonly IFormReviewService _formReviewService;
        private readonly ILogger<FormReviewsController> _logger;

        public FormReviewsController(IFormReviewService formReviewService, ILogger<FormReviewsController> logger)
        {
            _formReviewService = formReviewService;
            _logger = logger;
        }

        [HttpGet("moderator/{moderatorId}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReviewHistoryByModeratorAsync(Guid moderatorId, [FromQuery] QueriedFormReviewRequestDto payload)
        {
            var result = await _formReviewService.GetReviewHistoryByModeratorAsync(moderatorId, payload);

            if (result.Data == null || !result.Data.Any())
            {
                return NotFound(new
                {
                    Result = result,
                    Success = false,
                    Message = "No review history found for this moderator.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Review history retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("form-response/{formResponseId}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReviewHistoryByFormResponseAsync(Guid formResponseId)
        {
            var result = await _formReviewService.GetReviewHistoryByFormResponseAsync(formResponseId);

            if (!result.Any())
            {
                return NotFound(new
                {
                    Result = result,
                    Success = false,
                    Message = "No review history found for this form response.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Review history retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}