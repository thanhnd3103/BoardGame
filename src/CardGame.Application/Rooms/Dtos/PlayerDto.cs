namespace CardGame.Application.Rooms.Dtos;

public sealed record PlayerDto(
    string PlayerId,
    string DisplayName,
    int SeatIndex,
    bool IsConnected);
