using CardGame.Application.Rooms.Dtos;

namespace CardGame.Application.Rooms.LeaveRoom;

public sealed class LeaveRoomHandler(IRoomRepository roomRepository)
{
    public async Task<RoomDto?> HandleAsync(LeaveRoomCommand command)
    {
        var room = await roomRepository.GetByConnectionIdAsync(command.ConnectionId);
        if (room is null) return null;

        var playerId = await roomRepository.GetPlayerIdByConnectionAsync(command.ConnectionId);
        if (playerId.HasValue)
            room.RemovePlayer(playerId.Value);

        await roomRepository.UnmapConnectionAsync(command.ConnectionId);

        if (room.IsEmpty)
        {
            await roomRepository.RemoveAsync(room.Code);
            return null;
        }

        await roomRepository.SaveAsync(room);
        return room.ToDto();
    }
}
