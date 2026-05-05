using CardGame.Domain.Common;

namespace CardGame.Domain.GameEngine;

public abstract record GameEvent;

public sealed record CardsPlayedEvent(PlayerId PlayerId, IReadOnlyList<Card> Cards) : GameEvent;
public sealed record PlayerPassedEvent(PlayerId PlayerId) : GameEvent;
public sealed record TrickWonEvent(PlayerId WinnerId) : GameEvent;
public sealed record PlayerFinishedEvent(PlayerId PlayerId, int Place) : GameEvent;
public sealed record GameOverEvent(IReadOnlyList<(PlayerId PlayerId, int Place)> Rankings) : GameEvent;
public sealed record InstantWinEvent(PlayerId PlayerId, IReadOnlyList<Card> Hand) : GameEvent;
public sealed record NewTrickEvent(PlayerId LeadPlayerId) : GameEvent;
