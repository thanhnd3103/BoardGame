namespace CardGame.Application.Rooms.JoinRoom;

public sealed record JoinRoomCommand(
    string RoomCode,
    string DisplayName,
    string ConnectionId);
