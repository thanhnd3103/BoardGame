using CardGame.Application.Games.Dtos;

namespace CardGame.Application.Games.PlayCards;

public sealed record PlayCardsCommand(
    string ConnectionId,
    IReadOnlyList<CardDto> Cards);
