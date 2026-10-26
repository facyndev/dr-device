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

    public Task<int> DeleteExpiredBatchAsync(
        DateTime cutoffUtc, int batchSize, CancellationToken cancellationToken = default)
    {
        if (cutoffUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("La fecha de corte debe estar en UTC.", nameof(cutoffUtc));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        // Una sola sentencia evita una ventana entre la selección y el borrado.
        // SKIP LOCKED permite omitir cuentas que estén siendo reactivadas o procesadas.
        // El historial se conserva: las FK de órdenes y ofertas usan Restrict.
        return _applicationDbcontext.Database.ExecuteSqlInterpolatedAsync($"""
            WITH candidates AS (
                SELECT u."Id"
                FROM "Users" AS u
                WHERE u."DeletedAt" IS NOT NULL AND u."DeletedAt" <= {cutoffUtc}
                  AND NOT EXISTS (
                      SELECT 1 FROM "Orders" AS o
                      WHERE o."UserId" = u."Id" OR o."TechnicianId" = u."Id")
                  AND NOT EXISTS (
                      SELECT 1 FROM "Offers" AS o WHERE o."TechnicianId" = u."Id")
                ORDER BY u."DeletedAt", u."Id"
                LIMIT {batchSize}
                FOR UPDATE OF u SKIP LOCKED
            )
            DELETE FROM "Users" AS u
            USING candidates AS c
            WHERE u."Id" = c."Id"
              AND u."DeletedAt" IS NOT NULL AND u."DeletedAt" <= {cutoffUtc}
            """, cancellationToken);
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
