using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class UserCleanupJob(
    IUserRepository userRepository,
    IOptions<UserCleanupOptions> options,
    TimeProvider clock,
    ILogger<UserCleanupJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var settings = options.Value;
        var cutoff = clock.GetUtcNow().UtcDateTime.AddDays(-settings.RetentionDays);
        var deleted = await userRepository.DeleteExpiredBatchAsync(
            cutoff, settings.BatchSize, context.CancellationToken);

        logger.LogInformation(
            "Limpieza de usuarios: {DeletedCount} cuentas eliminadas; límite {BatchSize}; fecha de corte UTC {Cutoff}.",
            deleted, settings.BatchSize, cutoff);
    }
}
