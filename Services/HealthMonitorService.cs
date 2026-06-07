using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using vodongha.Data;

namespace vodongha.Services;

public record HealthMetricSnapshot(
    DateTime Timestamp,
    long MemoryMb,
    double DbPingMs,
    int ThreadCount,
    bool DbHealthy
);

public class HealthMonitorService : IHostedService, IDisposable
{
    private const int MaxSnapshots = 24; // ~12 minutes of history at 30s interval
    private readonly LinkedList<HealthMetricSnapshot> _snapshots = new();
    private readonly object _lock = new();
    private Timer? _timer;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public DateTime StartedAt { get; } = DateTime.UtcNow;

    public HealthMonitorService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Collect immediately, then every 30 seconds
        _timer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        return Task.CompletedTask;
    }

    private async void CollectMetrics(object? state)
    {
        try
        {
            Process process = Process.GetCurrentProcess();
            long memoryMb = process.WorkingSet64 / 1024 / 1024;
            int threadCount = process.Threads.Count;

            double dbPingMs = 0;
            bool dbHealthy = false;

            try
            {
                using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
                Stopwatch sw = Stopwatch.StartNew();
                await using AppDbContext db = await _dbFactory.CreateDbContextAsync(cts.Token);
                System.Data.Common.DbConnection conn = db.Database.GetDbConnection();
                await conn.OpenAsync(cts.Token);
                await using System.Data.Common.DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.CommandTimeout = 5;
                await cmd.ExecuteScalarAsync(cts.Token);
                sw.Stop();
                dbPingMs = Math.Round(sw.Elapsed.TotalMilliseconds, 1);
                dbHealthy = true;
            }
            catch
            {
                dbHealthy = false;
            }

            HealthMetricSnapshot snapshot = new(
                Timestamp: DateTime.UtcNow,
                MemoryMb: memoryMb,
                DbPingMs: dbPingMs,
                ThreadCount: threadCount,
                DbHealthy: dbHealthy
            );

            lock (_lock)
            {
                _snapshots.AddLast(snapshot);
                if (_snapshots.Count > MaxSnapshots)
                {
                    _snapshots.RemoveFirst();
                }
            }
        }
        catch { /* never crash the timer */ }
    }

    public IReadOnlyList<HealthMetricSnapshot> GetSnapshots()
    {
        lock (_lock)
        {
            return _snapshots.ToList();
        }
    }

    public HealthMetricSnapshot? Latest
    {
        get
        {
            lock (_lock)
            {
                return _snapshots.Last?.Value;
            }
        }
    }

    public TimeSpan Uptime => DateTime.UtcNow - StartedAt;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
