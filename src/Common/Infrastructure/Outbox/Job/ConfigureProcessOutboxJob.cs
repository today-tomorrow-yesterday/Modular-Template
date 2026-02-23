using Microsoft.Extensions.Options;
using Quartz;

namespace Rtl.Core.Infrastructure.Outbox.Job;

public sealed class ConfigureProcessOutboxJob<TJob>(IOptions<OutboxOptions> outboxOptions)
    : IConfigureOptions<QuartzOptions>
    where TJob : IJob
{
    private readonly OutboxOptions _outboxOptions = outboxOptions.Value;

    public void Configure(QuartzOptions options)
    {
        var jobName = typeof(TJob).FullName!;

        options
            .AddJob<TJob>(configure => configure
                .WithIdentity(jobName)
                .StoreDurably()) // Required when using IConfigureOptions pattern
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithIntervalInSeconds(_outboxOptions.IntervalInSeconds).RepeatForever()));
    }
}
