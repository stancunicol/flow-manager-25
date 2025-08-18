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
        Task<IEnumerable<TeamResponseDto>> GetAllTeamsAsync();
        Task<PagedResponseDto<TeamResponseDto>> GetAllTeamsQueriedAsync(QueriedTeamRequestDto payload);
        Task<TeamResponseDto> GetTeamByIdAsync(Guid id);
        Task<TeamResponseDto> GetTeamByNameAsync(string name);
        Task<TeamResponseDto> GetTeamWithUsersAsync(Guid id); // Returnează același DTO dar cu Users populat
        Task<TeamResponseDto> AddTeamAsync(PostTeamRequestDto payload);
        Task<TeamResponseDto> UpdateTeamAsync(Guid id, PatchTeamRequestDto payload);
        Task<TeamResponseDto> DeleteTeamAsync(Guid id);
        Task<TeamResponseDto> RestoreTeamAsync(Guid id);
    }
}
