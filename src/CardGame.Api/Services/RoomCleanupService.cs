using CardGame.Infrastructure.Persistence;

namespace CardGame.Api.Services;

public sealed class RoomCleanupService(InMemoryRoomRepository roomRepository, ILogger<RoomCleanupService> logger)
    : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaxIdleTime = TimeSpan.FromMinutes(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CleanupInterval, stoppingToken);
            CleanupStaleRooms();
        }
    }

    private void CleanupStaleRooms()
    {
        var cutoff = DateTime.UtcNow - MaxIdleTime;
        var staleRooms = roomRepository.GetAllRooms()
            .Where(r => r.CreatedAt < cutoff && r.Players.All(p => !p.IsConnected))
            .ToList();

        foreach (var room in staleRooms)
        {
            roomRepository.RemoveAsync(room.Code).GetAwaiter().GetResult();
            logger.LogInformation("Cleaned up stale room {RoomCode}", room.Code.Value);
        }
    }
}
