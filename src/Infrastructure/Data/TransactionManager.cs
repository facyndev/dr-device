using Application.Interfaces;

namespace Infrastructure.Data;

public class TransactionManager(ApplicationDbContext context) : ITransactionManager
{
    public Task ExecuteAsync(Func<Task> operation) => ExecuteAsync(async () =>
    {
        await operation();
        return true;
    });

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            // No conservar cambios en memoria que luego puedan guardarse por accidente.
            context.ChangeTracker.Clear();
            throw;
        }
    }
}
