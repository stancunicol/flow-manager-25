using FlowManager.Shared.DTOs.Requests.Component;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.IServices
{
    public interface IComponentService
    {
        Task<PagedResponseDto<ComponentResponseDto>> GetComponentsQueriedAsync(QueriedComponentRequestDto payload);
        Task<ComponentResponseDto> GetComponentByIdAsync(Guid id);
        Task<ComponentResponseDto> PostComponentAsync(PostComponentRequestDto payload);
        Task<ComponentResponseDto> DeleteComponentAsync(Guid id);
        Task<ComponentResponseDto> PatchComponentAsync(Guid id, PatchComponentRequestDto payload);
    }
}
