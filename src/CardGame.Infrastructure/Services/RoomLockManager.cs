using System.Collections.Concurrent;

namespace CardGame.Infrastructure.Services;

public sealed class RoomLockManager
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public SemaphoreSlim GetLock(string roomCode)
    {
        return _locks.GetOrAdd(roomCode, _ => new SemaphoreSlim(1, 1));
    }

    public void RemoveLock(string roomCode)
    {
        if (_locks.TryRemove(roomCode, out var semaphore))
            semaphore.Dispose();
    }
}
