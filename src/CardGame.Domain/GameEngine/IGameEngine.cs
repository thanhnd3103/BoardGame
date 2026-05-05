using CardGame.Domain.Common;
using CardGame.Domain.Rooms;

namespace CardGame.Domain.GameEngine;

public interface IGameEngine
{
    GameType GameType { get; }
    (IGameState State, IReadOnlyList<GameEvent> Events) CreateInitialState(
        IReadOnlyList<Player> players, bool isFirstRound, PlayerId? previousWinnerId);
    MoveResult ProcessMove(IGameState state, PlayerId playerId, IGameMove move);
    bool IsGameOver(IGameState state);
    GameResult GetResult(IGameState state);
    GameStateView GetPlayerView(IGameState state, PlayerId playerId);
}
