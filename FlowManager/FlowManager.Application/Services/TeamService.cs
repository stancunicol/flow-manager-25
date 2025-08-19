using FlowManager.Application.Interfaces; // Schimbat din IServices
using FlowManager.Domain.Entities;
using FlowManager.Domain.Exceptions;
using FlowManager.Domain.IRepositories;
using FlowManager.Infrastructure.Utils;
using FlowManager.Application.Utils; // ADĂUGAT pentru PatchHelper
using FlowManager.Shared.DTOs.Requests.Team;
using FlowManager.Shared.DTOs.Responses;
using FlowManager.Shared.DTOs.Responses.Team;
using FlowManager.Shared.DTOs.Responses.User;
using FlowManager.Shared.DTOs.Responses.Role; // ADĂUGAT pentru RoleResponseDto
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowManager.Application.IServices;

namespace FlowManager.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;

        public TeamService(ITeamRepository teamRepository, IUserRepository userRepository)
        {
            _teamRepository = teamRepository;
            _userRepository = userRepository;
        }

        // Helper method pentru mapping
        private TeamResponseDto MapToTeamResponseDto(Team team)
        {
            return new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt,
                DeletedAt = team.DeletedAt,
                UsersCount = team.Users?.Count ?? 0
            };
        }

        public async Task<IEnumerable<TeamResponseDto>> GetAllTeamsAsync()
        {
            var teams = await _teamRepository.GetAllTeamsAsync();
            return teams.Select(MapToTeamResponseDto);
        }

        public async Task<PagedResponseDto<TeamResponseDto>> GetAllTeamsQueriedAsync(QueriedTeamRequestDto payload)
        {
            (List<Team> result, int totalCount) = await _teamRepository.GetAllTeamsQueriedAsync(
                payload.Name,
                payload.QueryParams?.ToQueryParams());

            return new PagedResponseDto<TeamResponseDto>
            {
                Data = result.Select(MapToTeamResponseDto),
                Page = payload.QueryParams?.Page ?? 1,
                PageSize = payload.QueryParams?.PageSize ?? totalCount,
                TotalCount = totalCount
            };
        }

        public async Task<TeamResponseDto> GetTeamByIdAsync(Guid id)
        {
            var team = await _teamRepository.GetTeamByIdAsync(id);

            if (team == null)
            {
                throw new EntryNotFoundException($"Team with id {id} was not found.");
            }

            return MapToTeamResponseDto(team);
        }

        public async Task<TeamResponseDto> AddTeamAsync(PostTeamRequestDto payload)
        {
            // Verifică dacă există deja o echipă cu același nume
            var existingTeam = await _teamRepository.GetTeamByNameAsync(payload.Name);
            if (existingTeam != null)
            {
                throw new UniqueConstraintViolationException($"Team with name {payload.Name} already exists.");
            }

            Team teamToAdd = new Team
            {
                Name = payload.Name,
            };

            // Salvează echipa mai întâi
            var result = await _teamRepository.AddTeamAsync(teamToAdd);

            // Apoi adaugă userii la echipă (dacă sunt specificați)
            if (payload.UserIds != null && payload.UserIds.Any())
            {
                foreach (Guid userId in payload.UserIds)
                {
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        throw new EntryNotFoundException($"User with id {userId} was not found (trying to create a team).");
                    }

                    // Verifică dacă user-ul nu este deja într-o altă echipă
                    if (user.TeamId.HasValue)
                    {
                        throw new InvalidOperationException($"User {user.Name} is already assigned to another team.");
                    }

                    user.TeamId = result.Id; // Folosește ID-ul echipei salvate
                    user.UpdatedAt = DateTime.UtcNow;
                }

                await _userRepository.SaveChangesAsync(); // Salvează modificările userilor
            }

            return MapToTeamResponseDto(result);
        }

        public async Task<TeamResponseDto> UpdateTeamAsync(Guid id, PatchTeamRequestDto payload)
        {
            var teamToUpdate = await _teamRepository.GetTeamByIdAsync(id);

            if (teamToUpdate == null)
            {
                throw new EntryNotFoundException($"Team with id {id} was not found.");
            }

            // Actualizează proprietățile de bază folosind PatchHelper
            if (!string.IsNullOrEmpty(payload.Name))
            {
                teamToUpdate.Name = payload.Name;
            }

            

            teamToUpdate.UpdatedAt = DateTime.UtcNow;

            // Gestionează userii (similar cu rolurile din UserService)
            if (payload.UserIds != null)
            {
                // 1. "Șterge" toți userii din echipa curentă (setează TeamId = null)
                var currentUsers = await _userRepository.GetUsersByTeamIdAsync(id);
                foreach (User user in currentUsers)
                {
                    user.TeamId = null;
                    user.UpdatedAt = DateTime.UtcNow;
                }

                // 2. Adaugă userii noi în echipă
                foreach (Guid userId in payload.UserIds)
                {
                    var user = await _userRepository.GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        throw new EntryNotFoundException($"User with id {userId} was not found (trying to update team).");
                    }

                    // Verifică dacă user-ul este disponibil (nu e în altă echipă)
                    if (user.TeamId.HasValue && user.TeamId != id)
                    {
                        throw new InvalidOperationException($"User {user.Name} is already assigned to another team.");
                    }

                    user.TeamId = id;
                    user.UpdatedAt = DateTime.UtcNow;
                }

                await _userRepository.SaveChangesAsync(); // Salvează modificările userilor
            }

            await _teamRepository.SaveChangesAsync();

            return MapToTeamResponseDto(teamToUpdate);
        }

        public async Task<TeamResponseDto> DeleteTeamAsync(Guid id)
        {
            var teamToDelete = await _teamRepository.GetTeamByIdAsync(id);

            if (teamToDelete == null)
            {
                throw new EntryNotFoundException($"Team with id {id} was not found.");
            }

            // Elimină toți userii din echipă înainte de ștergere
            var users = await _userRepository.GetUsersByTeamIdAsync(id);
            foreach (var user in users)
            {
                user.TeamId = null;
                user.UpdatedAt = DateTime.UtcNow;
            }

            teamToDelete.DeletedAt = DateTime.UtcNow;
            teamToDelete.UpdatedAt = DateTime.UtcNow;

            await _teamRepository.SaveChangesAsync();

            return new TeamResponseDto
            {
                Id = teamToDelete.Id,
                Name = teamToDelete.Name,
                CreatedAt = teamToDelete.CreatedAt,
                UpdatedAt = teamToDelete.UpdatedAt,
                DeletedAt = teamToDelete.DeletedAt,
                UsersCount = 0 // După ștergere nu mai are useri
            };
        }

        public async Task<TeamResponseDto> RestoreTeamAsync(Guid id)
        {
            var teamToRestore = await _teamRepository.GetTeamByIdAsync(id, includeDeleted: true);

            if (teamToRestore == null)
            {
                throw new EntryNotFoundException($"Team with id {id} was not found.");
            }

            teamToRestore.DeletedAt = null;
            teamToRestore.UpdatedAt = DateTime.UtcNow;

            await _teamRepository.SaveChangesAsync();

            return MapToTeamResponseDto(teamToRestore);
        }

        public async Task<TeamResponseDto> GetTeamByNameAsync(string name)
        {
            Team? team = await _teamRepository.GetTeamByNameAsync(name);

            if (team == null)
            {
                throw new EntryNotFoundException($"Team with name {name} was not found.");
            }

            return MapToTeamResponseDto(team);
        }

        // Metodă helper pentru a obține echipa cu toți userii (versiunea detaliată)
        public async Task<TeamResponseDto> GetTeamWithUsersAsync(Guid id)
        {
            var team = await _teamRepository.GetTeamWithUsersAsync(id);

            if (team == null)
            {
                throw new EntryNotFoundException($"Team with id {id} was not found.");
            }

            return new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt,
                DeletedAt = team.DeletedAt,
                UsersCount = team.Users?.Count ?? 0,
                Users = team.Users?.Select(u => new UserResponseDto // Populează Users când sunt cerute detaliile
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    UserName = u.UserName,
                    TeamId = u.TeamId,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    DeletedAt = u.DeletedAt,
                    Roles = u.Roles?.Select(r => new RoleResponseDto
                    {
                        Id = r.RoleId,
                        Name = r.Role?.Name ?? string.Empty
                    }).ToList()
                }).ToList()
            };
        }
    }
}