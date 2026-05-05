using CardGame.Domain.Common;

namespace CardGame.Domain.GameEngine;

public sealed record GameStateView
{
    public required IReadOnlyList<Card> Hand { get; init; }
    public required IReadOnlyList<Card> PlayArea { get; init; }
    public required PlayerId CurrentTurnPlayerId { get; init; }
    public required IReadOnlyList<PlayerInfo> Players { get; init; }
    public required IReadOnlyList<(PlayerId PlayerId, int Place)> FinishOrder { get; init; }
    public required bool IsGameOver { get; init; }
}

public sealed record PlayerInfo(PlayerId Id, string DisplayName, int CardCount, bool HasPassed, bool HasFinished);
