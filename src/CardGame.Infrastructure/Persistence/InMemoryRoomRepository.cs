using System.Collections.Concurrent;
using CardGame.Application.Rooms;
using CardGame.Domain.Common;
using CardGame.Domain.Rooms;

namespace CardGame.Infrastructure.Persistence;

public sealed class InMemoryRoomRepository : IRoomRepository
{
    private readonly ConcurrentDictionary<string, Room> _rooms = new();
    private readonly ConcurrentDictionary<string, (RoomCode Code, PlayerId PlayerId)> _connectionMap = new();

    public Task<Room?> GetByCodeAsync(RoomCode code)
    {
        _rooms.TryGetValue(code.Value, out var room);
        return Task.FromResult(room);
    }

    public Task<Room?> GetByConnectionIdAsync(string connectionId)
    {
        if (_connectionMap.TryGetValue(connectionId, out var mapping))
        {
            _rooms.TryGetValue(mapping.Code.Value, out var room);
            return Task.FromResult(room);
        }
        return Task.FromResult<Room?>(null);
    }

    public Task SaveAsync(Room room)
    {
        _rooms[room.Code.Value] = room;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(RoomCode code)
    {
        _rooms.TryRemove(code.Value, out _);
        return Task.CompletedTask;
    }

    public Task MapConnectionAsync(string connectionId, RoomCode code, PlayerId playerId)
    {
        _connectionMap[connectionId] = (code, playerId);
        return Task.CompletedTask;
    }

    public Task UnmapConnectionAsync(string connectionId)
    {
        _connectionMap.TryRemove(connectionId, out _);
        return Task.CompletedTask;
    }

    public Task<PlayerId?> GetPlayerIdByConnectionAsync(string connectionId)
    {
        if (_connectionMap.TryGetValue(connectionId, out var mapping))
            return Task.FromResult<PlayerId?>(mapping.PlayerId);
        return Task.FromResult<PlayerId?>(null);
    }

    public IEnumerable<Room> GetAllRooms() => _rooms.Values;
}
