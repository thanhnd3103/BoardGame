using CardGame.Domain.Common;
using CardGame.Domain.GameEngine;

namespace CardGame.Application.Games.Dtos;

public static class GameDtoMappers
{
    public static GameStateDto ToDto(this GameStateView view) =>
        new(
            view.Hand.Select(c => c.ToDto()).ToList(),
            view.PlayArea.Select(c => c.ToDto()).ToList(),
            view.CurrentTurnPlayerId.ToString(),
            view.Players.Select(p => new GamePlayerDto(
                p.Id.ToString(),
                p.DisplayName,
                p.CardCount,
                p.HasPassed,
                p.HasFinished)).ToList(),
            view.FinishOrder.Select(f => new FinishEntryDto(f.PlayerId.ToString(), f.Place)).ToList(),
            view.IsGameOver);

    public static CardDto ToDto(this Card card) =>
        new(card.Rank.ToString(), card.Suit.ToString());
}
