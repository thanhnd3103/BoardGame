using CardGame.Domain.Common;
using CardGame.Domain.Rooms;

namespace CardGame.Application.Rooms;

public interface IRoomRepository
{
    Task<Room?> GetByCodeAsync(RoomCode code);
    Task<Room?> GetByConnectionIdAsync(string connectionId);
    Task SaveAsync(Room room);
    Task RemoveAsync(RoomCode code);
    Task MapConnectionAsync(string connectionId, RoomCode code, PlayerId playerId);
    Task UnmapConnectionAsync(string connectionId);
    Task<PlayerId?> GetPlayerIdByConnectionAsync(string connectionId);
}
