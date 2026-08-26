using Application.Abstractions.Persistence;
using Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException
                      {
                          SqlState: PostgresErrorCodes.ExclusionViolation,
                          ConstraintName: "ex_bookings_no_overlap"
                      })
        {
            // гарантує відсутність подвійного бронювання при конкурентних запитах.
            throw new ConflictException(
            "Conference room is already booked for the selected time period.");
        }
    }
}