using CardGame.Domain.Rooms;

namespace CardGame.Application.Rooms.Dtos;

public static class DtoMappers
{
    public static RoomDto ToDto(this Room room) =>
        new(
            room.Code.Value,
            room.GameType.ToString(),
            room.Status.ToString(),
            room.Players.Select(p => p.ToDto()).ToList(),
            room.HostPlayerId.ToString(),
            room.MaxPlayers);

    public static PlayerDto ToDto(this Player player) =>
        new(
            player.Id.ToString(),
            player.DisplayName,
            player.SeatIndex,
            player.IsConnected);
}
