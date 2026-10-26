using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Exceptions;
using Infrastructure.Data.Configurations;
using Npgsql;

namespace Infrastructure.Data;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext applicationDbcontext)
        : base(applicationDbcontext)
    {
    }

    public async Task<User?> GetByEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalizedEmail = email.ToLowerInvariant();
        return await _dbSet.FirstOrDefaultAsync(user =>
            EF.Property<string>(user, UserConfiguration.NormalizedEmail) == normalizedEmail);
    }

    public async Task<User?> GetByIdForUpdateAsync(Guid userId)
    {
        if (_applicationDbcontext.Database.CurrentTransaction is null)
            throw new InvalidOperationException("El bloqueo del usuario requiere una transacción activa.");

        var user = await _dbSet
            .FromSqlInterpolated($"SELECT * FROM \"Users\" WHERE \"Id\" = {userId} FOR UPDATE")
            .SingleOrDefaultAsync();
        // Refrescar también si el contexto ya había consultado este usuario antes del bloqueo.
        if (user is not null)
            await _applicationDbcontext.Entry(user).ReloadAsync();
        return user;
    }

    public async Task<bool> HasOrdersOrOffersAsync(Guid userId)
    {
        return await _applicationDbcontext.Orders.AnyAsync(order =>
                   order.UserId == userId || order.TechnicianId == userId)
            || await _applicationDbcontext.Offers.AnyAsync(offer => offer.TechnicianId == userId);
    }

    public override async Task<User> Create(User entity)
    {
        try
        {
            return await base.Create(entity);
        }
        catch (DbUpdateException exception) when (IsDuplicateEmail(exception))
        {
            throw new ConflictException("Ya existe un usuario con ese correo electrónico.", exception);
        }
    }

    public override async Task Update(User entity)
    {
        try
        {
            await base.Update(entity);
        }
        catch (DbUpdateException exception) when (IsDuplicateEmail(exception))
        {
            throw new ConflictException("Ya existe un usuario con ese correo electrónico.", exception);
        }
    }

    private static bool IsDuplicateEmail(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: UserConfiguration.EmailIndex
        };
}
