using CardGame.Application.Common;
using CardGame.Domain.GameEngine;

namespace CardGame.Application.Rooms.StartGame;

public sealed class StartGameHandler(IRoomRepository roomRepository, IGameEngineFactory engineFactory)
{
    public async Task<(IGameState State, IReadOnlyList<GameEvent> Events, string RoomCode)> HandleAsync(
        StartGameCommand command)
    {
        var room = await roomRepository.GetByConnectionIdAsync(command.ConnectionId)
            ?? throw new InvalidOperationException("Room not found.");

        var playerId = await roomRepository.GetPlayerIdByConnectionAsync(command.ConnectionId)
            ?? throw new InvalidOperationException("Player not found.");

        if (!room.IsHost(playerId))
            throw new InvalidOperationException("Only the host can start the game.");

        var isFirstRound = room.RoundNumber == 0;
        room.RoundNumber++;
        room.StartGame();

        var engine = engineFactory.Create(room.GameType);
        var (state, events) = engine.CreateInitialState(room.Players, isFirstRound, room.LastRoundWinnerId);
        room.ActiveGameState = state;

        await roomRepository.SaveAsync(room);

        return (state, events, room.Code.Value);
    }
}
