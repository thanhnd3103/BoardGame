using CardGame.Application.Common;
using CardGame.Application.Rooms.Dtos;
using CardGame.Domain.Common;
using CardGame.Domain.Rooms;

namespace CardGame.Application.Rooms.CreateRoom;

public sealed class CreateRoomHandler(IRoomRepository roomRepository, IRoomCodeGenerator codeGenerator)
{
    public async Task<(RoomDto Room, string PlayerId)> HandleAsync(CreateRoomCommand command)
    {
        var code = codeGenerator.Generate();
        var room = new Room(code, command.GameType);

        var playerId = PlayerId.New();
        room.AddPlayer(playerId, command.DisplayName, command.ConnectionId);

        await roomRepository.SaveAsync(room);
        await roomRepository.MapConnectionAsync(command.ConnectionId, code, playerId);

        return (room.ToDto(), playerId.ToString());
    }
}
