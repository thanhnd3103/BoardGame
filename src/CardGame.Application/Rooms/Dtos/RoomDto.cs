namespace CardGame.Application.Rooms.Dtos;

public sealed record RoomDto(
    string RoomCode,
    string GameType,
    string Status,
    IReadOnlyList<PlayerDto> Players,
    string HostPlayerId,
    int MaxPlayers);
