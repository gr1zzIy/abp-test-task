using Domain.Entities;

namespace Application.Abstractions.Persistence;

public interface IServiceRepository
{
    Task<List<Service>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default);
}