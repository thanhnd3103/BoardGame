using CardGame.Application.Rooms.Dtos;
using CardGame.Domain.Common;
using CardGame.Domain.Rooms;

namespace CardGame.Application.Rooms.JoinRoom;

public sealed class JoinRoomHandler(IRoomRepository roomRepository)
{
    public async Task<(RoomDto Room, string PlayerId)> HandleAsync(JoinRoomCommand command)
    {
        var code = new RoomCode(command.RoomCode);
        var room = await roomRepository.GetByCodeAsync(code)
            ?? throw new InvalidOperationException("Room not found.");

        var playerId = PlayerId.New();
        room.AddPlayer(playerId, command.DisplayName, command.ConnectionId);

        await roomRepository.SaveAsync(room);
        await roomRepository.MapConnectionAsync(command.ConnectionId, code, playerId);

        return (room.ToDto(), playerId.ToString());
    }
}
