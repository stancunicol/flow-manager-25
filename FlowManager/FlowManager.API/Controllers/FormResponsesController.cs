using FlowManager.Application.Interfaces;
using FlowManager.Shared.DTOs.Requests.FormResponse;
using Microsoft.AspNetCore.Mvc;

namespace FlowManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormResponsesController : ControllerBase
    {
        private readonly IFormResponseService _formResponseService;
        private readonly ILogger<FormResponsesController> _logger;

        public FormResponsesController(IFormResponseService formResponseService, ILogger<FormResponsesController> logger)
        {
            _formResponseService = formResponseService ?? throw new ArgumentNullException(nameof(formResponseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllFormResponsesAsync()
        {
            var result = await _formResponseService.GetAllFormResponsesAsync();

            if(result.Count == 0)
            {
                return NotFound(new
                {
                    Result = new List<FormResponseResponseDto>(),
                    Success = false,
                    Message = "No form responses found.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "All form responses retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFormResponsesQueriedAsync([FromQuery] QueriedFormResponseRequestDto payload)
        {
            var result = await _formResponseService.GetAllFormResponsesQueriedAsync(payload);

            if (result.Data == null || !result.Data.Any())
            {
                return NotFound(new
                {
                    Result = new List<FormResponseResponseDto>(),
                    Success = false,
                    Message = "No form responses found.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Form responses retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFormResponseByIdAsync(Guid id)
        {
            var result = await _formResponseService.GetFormResponseByIdAsync(id);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Form response retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("assigned-to-moderator/{moderatorId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFormResponsesAssignedToModeratorAsync(Guid moderatorId, [FromQuery] QueriedFormResponseRequestDto payload)
        {
            var result = await _formResponseService.GetFormResponsesAssignedToModeratorAsync(moderatorId, payload);

            if (result.Data == null || !result.Data.Any())
            {
                return NotFound(new
                {
                    Result = new List<FormResponseResponseDto>(),
                    Success = false,
                    Message = "No form responses assigned to this moderator.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Assigned form responses retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostFormResponseAsync([FromBody] PostFormResponseRequestDto payload)
        {
            var result = await _formResponseService.PostFormResponseAsync(payload);

            return Created($"/api/formresponses/{result.Id}", new
            {
                Result = result,
                Success = true,
                Message = "Form response created successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchFormResponseAsync(Guid id, [FromBody] PatchFormResponseRequestDto payload)
        {
            payload.Id = id;

            var result = await _formResponseService.PatchFormResponseAsync(payload);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Form response updated successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFormResponseAsync(Guid id)
        {
            var result = await _formResponseService.DeleteFormResponseAsync(id);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Form response deleted successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFormResponsesByUserAsync(Guid userId)
        {
            var result = await _formResponseService.GetFormResponsesByUserAsync(userId);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Form responses by user retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("step/{stepId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFormResponsesByStepAsync(Guid stepId)
        {
            var result = await _formResponseService.GetFormResponsesByStepAsync(stepId);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Form responses by step retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("template/{formTemplateId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFormResponsesByTemplateAsync(Guid formTemplateId)
        {
            var result = await _formResponseService.GetFormResponsesByTemplateAsync(formTemplateId);

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Form responses by template retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}