using CardGame.Application.Common;
using CardGame.Application.Rooms;
using CardGame.Domain.GameEngine;
using CardGame.Domain.TienLen;

namespace CardGame.Application.Games.PassTurn;

public sealed class PassTurnHandler(IRoomRepository roomRepository, IGameEngineFactory engineFactory)
{
    public async Task<(MoveResult Result, string RoomCode)> HandleAsync(PassTurnCommand command)
    {
        var room = await roomRepository.GetByConnectionIdAsync(command.ConnectionId)
            ?? throw new InvalidOperationException("Room not found.");

        var playerId = await roomRepository.GetPlayerIdByConnectionAsync(command.ConnectionId)
            ?? throw new InvalidOperationException("Player not found.");

        if (room.ActiveGameState is null)
            throw new InvalidOperationException("No game in progress.");

        var engine = engineFactory.Create(room.GameType);
        var result = engine.ProcessMove(room.ActiveGameState, playerId, PassMove.Instance);

        if (result.IsValid && result.NewState is not null)
        {
            room.ActiveGameState = result.NewState;

            if (engine.IsGameOver(result.NewState))
            {
                var gameResult = engine.GetResult(result.NewState);
                room.LastRoundWinnerId = gameResult.WinnerId;
                room.EndGame();
            }

            await roomRepository.SaveAsync(room);
        }

        return (result, room.Code.Value);
    }
}
