using CardGame.Application.Common;
using CardGame.Application.Rooms;
using CardGame.Domain.Common;
using CardGame.Domain.GameEngine;
using CardGame.Domain.TienLen;

namespace CardGame.Application.Games.PlayCards;

public sealed class PlayCardsHandler(IRoomRepository roomRepository, IGameEngineFactory engineFactory)
{
    public async Task<(MoveResult Result, string RoomCode)> HandleAsync(PlayCardsCommand command)
    {
        var room = await roomRepository.GetByConnectionIdAsync(command.ConnectionId)
            ?? throw new InvalidOperationException("Room not found.");

        var playerId = await roomRepository.GetPlayerIdByConnectionAsync(command.ConnectionId)
            ?? throw new InvalidOperationException("Player not found.");

        if (room.ActiveGameState is null)
            throw new InvalidOperationException("No game in progress.");

        var cards = command.Cards.Select(c => new Card(
            Enum.Parse<Rank>(c.Rank),
            Enum.Parse<Suit>(c.Suit))).ToList();

        var engine = engineFactory.Create(room.GameType);
        var move = new PlayCardsMove(cards);
        var result = engine.ProcessMove(room.ActiveGameState, playerId, move);

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
