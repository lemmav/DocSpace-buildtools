﻿namespace ASC.Feed.Aggregator.Service;

public abstract class FeedBaseService : IHostedService, IDisposable
{
    protected virtual string LoggerName { get; set; } = "ASC.Feed";

    protected Timer Timer;
    protected volatile bool IsStopped;
    protected readonly ILog Logger;
    protected readonly FeedSettings FeedSettings;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly object LockObj = new object();

    public FeedBaseService(
        FeedSettings feedSettings,
        IServiceProvider serviceProvider,
        IOptionsMonitor<ILog> optionsMonitor)
    {
        FeedSettings = feedSettings;
        ServiceProvider = serviceProvider;
        Logger = optionsMonitor.Get(LoggerName);
    }

    public abstract Task StartAsync(CancellationToken cancellationToken);

    public abstract Task StopAsync(CancellationToken cancellationToken);

    public void Dispose()
    {
        if (Timer == null)
        {
            return;
        }

        var handle = new AutoResetEvent(false);

        if (!Timer.Dispose(handle))
        {
            throw new Exception("Timer already disposed");
        }

        handle.WaitOne();
    }
}
