using Microsoft.AspNetCore.Identity;
namespace FlowManager.Domain.Entities;

public class Role : IdentityRole<Guid>
{
    public string roleName { get; set; } = string.Empty;
}