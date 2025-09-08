using FlowManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Services;
using FlowManager.Application.IServices;
using FlowManager.Shared.DTOs.Requests.FlowStep;
using FlowManager.Shared.DTOs.Responses.Flow;
using FlowManager.Shared.DTOs.Responses.FlowStep;
using FlowManager.Shared.DTOs.Requests.FlowStep;

namespace FlowManager.API.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    //public class FlowStepController : Controller
    //{
    //    private readonly IFlowStepService _flowStepService;

    //    public FlowStepController(IFlowStepService flowStepService)
    //    {
    //        _flowStepService = flowStepService;
    //    }

    //    [HttpGet("queried")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //    public async Task<IActionResult> GetFlowStepsQueriedAsync([FromQuery] QueriedFlowStepRequestDto payload)
    //    {
    //        var result = await _flowStepService.GetAllFlowStepsQueriedAsync(payload);

    //        if (result.Data == null || !result.Data.Any())
    //        {
    //            return NotFound(new
    //            {
    //                Result = new List<FlowStepResponseDto>(),
    //                Message = "No flows found matching the criteria.",
    //                Success = false,
    //                Timestamp = DateTime.UtcNow
    //            });
    //        }

    //        return Ok(new
    //        {
    //            Result = result,
    //            Success = true,
    //            Message = "Flows retreived succesfully.",
    //            Timestamp = DateTime.UtcNow
    //        });
    //    }

    //    [HttpGet("")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //    public async Task<IActionResult> GetAllFlowStepsAsync()
    //    {
    //        var result = await _flowStepService.GetAllFlowStepsAsync();
    //        return Ok(new
    //        {
    //            Result = result,
    //            Success = true,
    //            Timestamp = DateTime.UtcNow
    //        });
    //    }

    //    [HttpGet("{id}")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //    public async Task<IActionResult> GetFlowStepByIdAsync(Guid id)
    //    {
    //        var result = await _flowStepService.GetFlowStepByIdAsync(id);
    //        return Ok(new
    //        {
    //            Result = result,
    //            Succes = true,
    //            Timestamp = DateTime.UtcNow
    //        });
    //    }

    //    [HttpPost]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //    public async Task<IActionResult> CreateFlowStepAsync([FromBody] PostFlowStepRequestDto payload)
    //    {
    //        var result = await _flowStepService.CreateFlowStepAsync(payload);
    //        return Ok(new
    //        {
    //            Result = result,
    //            Success = true,
    //            Message = "FlowStep created successfully.",
    //            Timestamp = DateTime.UtcNow
    //        });
    //    }

    //    [HttpPatch("{id}")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //    public async Task<IActionResult> UpdateFlowStepAsync(Guid id, [FromBody] PatchFlowStepRequestDto payload)
    //    {
    //        var result = await _flowStepService.UpdateFlowStepAsync(id, payload);
    //        return Ok(new
    //        {
    //            Result = result,
    //            Success = true,
    //            Message = "FlowStep updated successfully.",
    //            Timestamp = DateTime.UtcNow
    //        });
    //    }

    //    [HttpDelete("{id}")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //    public async Task<IActionResult> DeleteFlowStepAsync(Guid id)
    //    {
    //        var result = await _flowStepService.DeleteFlowStepAsync(id);
    //        return Ok(new
    //        {
    //            Result = result,
    //            Success = true,
    //            Message = "FlowStep deleted successfully.",
    //            Timestamp = DateTime.UtcNow
    //        });
    //    }
    //}
}
