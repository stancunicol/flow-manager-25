using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FlowManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService; 
        private readonly ILogger<Role> _logger;

        public RolesController(IRoleService roleService, ILogger<Role> logger)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            var result = await _roleService.GetAllRolesAsync();

            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Roles retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoleByIdQueriedAsync(Guid id)
        {
            var result = await _roleService.GetRoleByIdAsync(id);
            return Ok(new
            {
                Result = result,
                Success = true,
                Message = "Role retrieved successfully.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
