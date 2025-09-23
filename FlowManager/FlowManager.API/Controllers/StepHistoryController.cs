using FlowManager.Application.Interfaces;
using FlowManager.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using FlowManager.Shared.DTOs.Requests.StepHistory;
using FlowManager.Shared.DTOs.Responses.StepHistory;

namespace FlowManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StepHistoryController : ControllerBase
    {
        private readonly IStepHistoryService _stepHistoryService;
        private readonly ILogger<StepHistoryController> _logger;

        public StepHistoryController(IStepHistoryService stepHistoryService, ILogger<StepHistoryController> logger)
        {
            _stepHistoryService = stepHistoryService;
            _logger = logger;
        }

        [HttpGet("queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStepHistoriesQueriedAsync([FromQuery] QueriedStepHistoryRequestDto payload)
        {
            PagedResponseDto<StepHistoryResponseDto> result = await _stepHistoryService.GetStepHistoriesQueriedAsync(payload);

            if (result.Data == null || !result.Data.Any())
            {
                return NotFound(new
                {
                    Result = new List<StepHistoryResponseDto>(),
                    Message = "No step histories found matching the criteria.",
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Result = result,
                Message = "Step histories retrieved successfully",
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllStepHistoriesAsync()
        {
            IEnumerable<StepHistoryResponseDto> result = await _stepHistoryService.GetAllAsync();

            return Ok(new
            {
                Result = result,
                Message = "All step histories retrieved successfully",
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStepHistoryByIdAsync(Guid id)
        {
            StepHistoryResponseDto result = await _stepHistoryService.GetByIdAsync(id);

            return Ok(new
            {
                Result = result,
                Message = "Step history retrieved successfully",
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("change-name")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateStepHistoryForNameChangeAsync([FromBody] CreateStepHistoryRequestDto payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Result = (StepHistoryResponseDto?)null,
                    Message = "Invalid payload",
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            StepHistoryResponseDto result = await _stepHistoryService.CreateStepHistoryForNameChangeAsync(payload);

            return Ok(new
            {
                Result = result,
                Message = "Step history for name change created successfully",
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("move-users")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateStepHistoryForMoveUsersAsync([FromBody] CreateStepHistoryRequestDto payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Result = (StepHistoryResponseDto?)null,
                    Message = "Invalid payload",
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            StepHistoryResponseDto result = await _stepHistoryService.CreateStepHistoryForMoveUsersAsync(payload);

            return Ok(new
            {
                Result = result,
                Message = "Step history for user move created successfully",
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("create-department")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateStepHistoryForCreateDepartmentAsync([FromBody] CreateStepHistoryRequestDto payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Result = (StepHistoryResponseDto?)null,
                    Message = "Invalid payload",
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            StepHistoryResponseDto result = await _stepHistoryService.CreateStepHistoryForCreateDepartmentAsync(payload);

            return Ok(new
            {
                Result = result,
                Message = "Step history for department created successfully",
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("delete-department")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateStepHistoryForDeleteDepartmentAsync([FromBody] CreateStepHistoryRequestDto payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Result = (StepHistoryResponseDto?)null,
                    Message = "Invalid payload",
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            StepHistoryResponseDto result = await _stepHistoryService.CreateStepHistoryForDeleteDepartmentAsync(payload);

            return Ok(new
            {
                Result = result,
                Message = "Step history for department deletion created successfully",
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}