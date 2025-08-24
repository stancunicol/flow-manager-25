using FlowManager.Shared.DTOs.Requests;
using FlowManager.Shared.DTOs.Requests.Team;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.IServices
{
    public interface ITeamService
    {
        Task<PagedResponseDto<TeamResponseDto>> GetAllTeamsQueriedAsync(QueriedTeamRequestDto payload);
        Task<TeamResponseDto> GetTeamByIdAsync(Guid id);
        Task<TeamResponseDto> GetTeamByNameAsync(string name);
        Task<TeamResponseDto> GetTeamWithUsersAsync(Guid id); 
        Task<TeamResponseDto> AddTeamAsync(PostTeamRequestDto payload);
        Task<TeamResponseDto> UpdateTeamAsync(Guid id, PatchTeamRequestDto payload);
        Task<TeamResponseDto> DeleteTeamAsync(Guid id);
        Task<TeamResponseDto> RestoreTeamAsync(Guid id);
        Task<SplitUsersByTeamIdResponseDto> GetSplitUsersByTeamIdAsync(Guid id, QueriedTeamRequestDto payload);
    }
}
