using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace Infrastructure.Jobs;

public static class UserCleanupRegistration
{
    public static IServiceCollection AddUserCleanup(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<UserCleanupOptions>()
            .Bind(configuration.GetSection(UserCleanupOptions.SectionName))
            .Validate(settings => settings.IntervalSeconds > 0,
                "UserCleanup:IntervalSeconds debe ser mayor que cero.")
            .Validate(settings => settings.RetentionDays > 0
                && settings.RetentionDays < DateTime.UtcNow.Subtract(DateTime.MinValue).TotalDays,
                "UserCleanup:RetentionDays debe ser positivo y permitir calcular una fecha de corte válida.")
            .Validate(settings => settings.BatchSize > 0,
                "UserCleanup:BatchSize debe ser mayor que cero.")
            .ValidateOnStart();

        services.AddQuartz((scheduler, provider) =>
        {
            var settings = provider.GetRequiredService<IOptions<UserCleanupOptions>>().Value;
            var interval = TimeSpan.FromSeconds(settings.IntervalSeconds);
            var jobKey = new JobKey(nameof(UserCleanupJob));

            scheduler.AddJob<UserCleanupJob>(job => job.WithIdentity(jobKey));
            scheduler.AddTrigger(trigger => trigger
                .WithIdentity($"{nameof(UserCleanupJob)}-trigger")
                .ForJob(jobKey)
                .StartAt(provider.GetRequiredService<TimeProvider>().GetUtcNow().Add(interval))
                .WithSimpleSchedule(schedule => schedule
                    .WithInterval(interval)
                    .RepeatForever()
                    .WithMisfireHandlingInstructionNextWithRemainingCount()));
        });
        services.AddQuartzHostedService(settings => settings.WaitForJobsToComplete = true);
        return services;
    }
}
