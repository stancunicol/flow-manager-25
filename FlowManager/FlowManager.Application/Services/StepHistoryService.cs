using FlowManager.Application.Interfaces;
using FlowManager.Domain.Entities;
using FlowManager.Domain.IRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowManager.Shared.DTOs.Responses.StepHistory;
using FlowManager.Shared.DTOs.Requests.StepHistory;
using FlowManager.Shared.DTOs.Responses;

namespace FlowManager.Application.Services
{
    public class StepHistoryService : IStepHistoryService
    {
        private readonly IStepHistoryRepository _repository;
        private readonly IStepRepository _stepRepository;

        public StepHistoryService(IStepHistoryRepository repository, IStepRepository stepRepository)
        {
            _repository = repository;
            _stepRepository = stepRepository;
        }

        public async Task<PagedResponseDto<StepHistoryResponseDto>> GetStepHistoriesQueriedAsync(QueriedStepHistoryRequestDto? payload)
        {
            var query = await _repository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(payload.Action))
                query = query.Where(x => x.Action == payload.Action);

            var totalCount = query.Count();
            var page = payload.QueryParams.Page ?? 1;
            var pageSize = payload.QueryParams.PageSize ?? 10;

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new StepHistoryResponseDto
                {
                    Id = x.IdStepHistory,
                    StepId = x.StepId,
                    Action = x.Action,
                    Details = x.Details,
                    DateTime = x.DateTime
                })
                .ToList();

            return new PagedResponseDto<StepHistoryResponseDto>
            {
                Data = items,
                TotalCount = totalCount,
                Page = payload.QueryParams.Page ?? 1,
                PageSize = payload.QueryParams.PageSize ?? totalCount
            };
        }

        public async Task<IEnumerable<StepHistoryResponseDto>> GetAllAsync()
        {
            var stepHistories = await _repository.GetAllAsync();
            var stepHistoryResponses = stepHistories.Select(sh => new StepHistoryResponseDto
            {
                Id = sh.IdStepHistory,
                StepId = sh.StepId,
                Action = sh.Action,
                Details = sh.Details,
                DateTime = sh.DateTime
            });
            return stepHistoryResponses;
        }

        public async Task<StepHistoryResponseDto> GetByIdAsync(Guid id)
        {
            var stepHistory =  await _repository.GetByIdAsync(id);
            var stepHistoryResponse = new StepHistoryResponseDto
            {
                Id = id,
                Action = stepHistory.Action,
                Details = stepHistory.Details,
                DateTime = stepHistory.DateTime
            };
            return stepHistoryResponse;
        }

        public async Task<StepHistoryResponseDto> CreateStepHistoryForNameChangeAsync(CreateStepHistoryRequestDto payload)
        {
            var step = await _stepRepository.GetStepByIdAsync(payload.StepId);
            if (step == null) throw new Exception("Step not found");

            var detailsObj = new { OldName = payload.OldDepartmentName, NewName = payload.NewName, Date = DateTime.UtcNow };
            var detailsJson = System.Text.Json.JsonSerializer.Serialize(detailsObj);

            var history = new StepHistory
            {
                StepId = payload.StepId,
                Action = "Change Name",
                Details = detailsJson,
                DateTime = DateTime.UtcNow
            };
            await _repository.CreateAsync(history);

            return new StepHistoryResponseDto
            {

                StepId = history.StepId,
                Action = history.Action,
                Details = history.Details,
                DateTime = history.DateTime
            };
        }

        public async Task<StepHistoryResponseDto> CreateStepHistoryForMoveUsersAsync(CreateStepHistoryRequestDto payload)
        {
            var step = await _stepRepository.GetStepByIdAsync(payload.StepId);
            if (step == null) throw new Exception("Step not found");

            var detailsObj = new { Users = payload.Users, From = payload.FromDepartment, To = payload.ToDepartment, Date = DateTime.UtcNow };
            var detailsJson = System.Text.Json.JsonSerializer.Serialize(detailsObj);

            var history = new StepHistory
            {
                IdStepHistory = Guid.NewGuid(),
                StepId = payload.StepId,
                Action = "Move Users",
                Details = detailsJson,
                DateTime = DateTime.UtcNow
            };
            await _repository.CreateAsync(history);

            return new StepHistoryResponseDto
            {
                Id = history.IdStepHistory,
                StepId = history.StepId,
                Action = history.Action,
                Details = history.Details,
                DateTime = history.DateTime
            };
        }

        public async Task<StepHistoryResponseDto> CreateStepHistoryForCreateDepartmentAsync(CreateStepHistoryRequestDto payload)
        {
            var detailsObj = new { DepartmentName = payload.NewDepartmentName, Date = DateTime.UtcNow };
            var detailsJson = System.Text.Json.JsonSerializer.Serialize(detailsObj);

            var history = new StepHistory
            {
                IdStepHistory = Guid.NewGuid(),
                StepId = payload.StepId,
                Action = "Create Department",
                Details = detailsJson,
                DateTime = DateTime.UtcNow
            };
            await _repository.CreateAsync(history);

            return new StepHistoryResponseDto
            {
                Id = history.IdStepHistory,
                StepId = history.StepId,
                Action = history.Action,
                Details = history.Details,
                DateTime = history.DateTime
            };
        }

        public async Task<StepHistoryResponseDto> CreateStepHistoryForDeleteDepartmentAsync(CreateStepHistoryRequestDto payload)
        {
            var detailsObj = new { DepartmentName = payload.OldDepartmentName, Date = DateTime.UtcNow };
            var detailsJson = System.Text.Json.JsonSerializer.Serialize(detailsObj);

            var history = new StepHistory
            {
                IdStepHistory = Guid.NewGuid(),
                StepId = payload.StepId,
                Action = "Delete Department",
                Details = detailsJson,
                DateTime = DateTime.UtcNow
            };
            await _repository.CreateAsync(history);

            return new StepHistoryResponseDto
            {
                Id = history.IdStepHistory,
                StepId = history.StepId,
                Action = history.Action,
                Details = history.Details,
                DateTime = history.DateTime
            };
        }
    }
}