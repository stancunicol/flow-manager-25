using FlowManager.Shared.DTOs.Responses.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Shared.DTOs.Responses.Team
{
    public class SplitUsersByTeamIdResponseDto
    {
        public Guid TeamId { get; set; }

        public List<UserResponseDto> AssignedToTeamUsers { get; set; }
        public List<UserResponseDto> UnassignedToTeamUsers { get; set; }
        public int TotalCountAssigned { get; set; }
        public int TotalCountUnassigned { get; set; }
        public int TotalPages { get; set; }
    }
}
