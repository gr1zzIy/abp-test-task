using Application.Abstractions.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _dbContext;

    public ServiceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Service>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var idCollection = ids as IReadOnlyCollection<int> ?? ids.ToList();

        if (idCollection.Count == 0)
        {
            return [];
        }

        return await _dbContext.Services
            .Where(service => idCollection.Contains(service.Id))
            .ToListAsync(cancellationToken);
    }
}