using Microsoft.AspNetCore.Mvc;
using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;

namespace FlowManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormsController : ControllerBase
    {
        private readonly IFormService _formService;

        public FormsController(IFormService formService)
        {
            _formService = formService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Form>>> GetForms()
        {
            var forms = await _formService.GetAllFormsAsync();
            return Ok(forms);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Form>> GetForm(Guid id)
        {
            var form = await _formService.GetFormByIdAsync(id);
            if (form == null)
                return NotFound();

            return Ok(form);
        }

        [HttpGet("flow/{flowId}")]
        public async Task<ActionResult<IEnumerable<Form>>> GetFormsByFlow(Guid flowId)
        {
            var forms = await _formService.GetFormsByFlowAsync(flowId);
            return Ok(forms);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Form>>> GetFormsByUser(Guid userId)
        {
            var forms = await _formService.GetFormsByUserAsync(userId);
            return Ok(forms);
        }

        [HttpPost]
        public async Task<ActionResult<Form>> PostForm(Form form)
        {
            var createdForm = await _formService.CreateFormAsync(form);
            return CreatedAtAction(nameof(GetForm), new { id = createdForm.Id }, createdForm);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutForm(Guid id, Form form)
        {
            var updated = await _formService.UpdateFormAsync(id, form);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteForm(Guid id)
        {
            var deleted = await _formService.DeleteFormAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
