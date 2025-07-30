using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowManager.Infrastructure.Services
{
    public class FormService : IFormService
    {
        private readonly AppDbContext _context;

        public FormService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Form>> GetAllFormsAsync()
        {
            Console.WriteLine("[DEBUG] FormService.GetAllFormsAsync called");
            var forms = await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .ToListAsync();
            Console.WriteLine($"[DEBUG] FormService.GetAllFormsAsync returning {forms.Count()} forms");
            return forms;
        }

        public async Task<Form?> GetFormByIdAsync(Guid id)
        {
            return await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Form>> GetFormsByFlowAsync(Guid flowId)
        {
            return await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .Where(f => f.FlowId == flowId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Form>> GetFormsByUserAsync(Guid userId)
        {
            return await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task<Form> CreateFormAsync(Form form)
        {
            form.Id = Guid.NewGuid();
            form.CreatedAt = DateTime.UtcNow;
            form.UpdatedAt = DateTime.UtcNow;

            // Clear navigation properties to avoid EF tracking issues
            form.Flow = null!;
            form.User = null!;
            form.LastStep = null;

            _context.Forms.Add(form);
            await _context.SaveChangesAsync();

            // Return the form with navigation properties loaded
            return await _context.Forms
                .Include(f => f.Flow)
                .Include(f => f.User)
                .Include(f => f.LastStep)
                .FirstAsync(f => f.Id == form.Id);
        }

        public async Task<bool> UpdateFormAsync(Guid id, Form form)
        {
            if (id != form.Id)
                return false;

            form.UpdatedAt = DateTime.UtcNow;
            _context.Entry(form).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return _context.Forms.Any(e => e.Id == id);
            }
        }

        public async Task<bool> DeleteFormAsync(Guid id)
        {
            var form = await _context.Forms.FindAsync(id);
            if (form == null)
                return false;

            _context.Forms.Remove(form);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveFormStepAsync(Guid formId, Guid moderatorId)
        {
            try
            {
                var form = await _context.Forms
                    .Include(f => f.Flow)
                        .ThenInclude(flow => flow.FlowSteps)
                        .ThenInclude(fs => fs.Step)
                    .FirstOrDefaultAsync(f => f.Id == formId);

                if (form == null)
                {
                    Console.WriteLine($"[ERROR] Form with ID {formId} not found");
                    return false;
                }

                var orderedFlowSteps = form.Flow.FlowSteps.OrderBy(fs => fs.Step.CreatedAt).ToList();
                
                if (!orderedFlowSteps.Any())
                {
                    Console.WriteLine($"[ERROR] No steps found for flow {form.FlowId}");
                    return false;
                }

                // Find current step index
                var currentStepIndex = -1;
                if (form.LastStepId.HasValue)
                {
                    currentStepIndex = orderedFlowSteps.FindIndex(fs => fs.StepId == form.LastStepId.Value);
                }
                
                // If current step not found, assume we're at the first step
                if (currentStepIndex == -1)
                {
                    currentStepIndex = 0;
                }

                // Check if this is the last step
                if (currentStepIndex >= orderedFlowSteps.Count - 1)
                {
                    // This is the final step, mark form as approved
                    form.Status = FormStatus.Approved;
                    form.UpdatedAt = DateTime.UtcNow;
                    Console.WriteLine($"[INFO] Form {formId} approved at final step");
                }
                else
                {
                    // Move to next step
                    var nextStep = orderedFlowSteps[currentStepIndex + 1];
                    form.LastStepId = nextStep.StepId;
                    form.UpdatedAt = DateTime.UtcNow;
                    Console.WriteLine($"[INFO] Form {formId} moved to next step: {nextStep.Step.Name}");
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to approve form step: {ex.Message}");
                return false;
            }
        }
    }
}
