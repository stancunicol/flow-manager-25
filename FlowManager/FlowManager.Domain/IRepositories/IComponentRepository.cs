using FlowManager.Domain.Dtos;
using FlowManager.Domain.Entities;

namespace FlowManager.Domain.IRepositories
{
    public interface IComponentRepository
    {
        Task<(List<Component> Data, int TotalCount)> GetAllComponentsQueriedAsync(string? type,string? label, QueryParams? parameters);
        Task<Component?> GetComponentByIdAsync(Guid id);
        Task SaveChangesAsync();
        Task AddAsync(Component formTemplate);
    }
}
