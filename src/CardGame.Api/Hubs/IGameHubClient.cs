using CardGame.Application.Games.Dtos;
using CardGame.Application.Rooms.Dtos;

namespace CardGame.Api.Hubs;

public interface IGameHubClient
{
    Task RoomUpdated(RoomDto room);
    Task GameStarted(GameStateDto state);
    Task GameStateUpdated(GameStateDto state);
    Task PlayerFinished(string playerId, int place);
    Task GameOver(IReadOnlyList<FinishEntryDto> rankings);
    Task Error(string message);
}
