using CardGame.Domain.GameEngine;

namespace CardGame.Application.Rooms.CreateRoom;

public sealed record CreateRoomCommand(
    string DisplayName,
    GameType GameType,
    string ConnectionId);
