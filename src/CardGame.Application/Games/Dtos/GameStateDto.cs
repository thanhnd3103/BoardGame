namespace CardGame.Application.Games.Dtos;

public sealed record GameStateDto(
    IReadOnlyList<CardDto> Hand,
    IReadOnlyList<CardDto> PlayArea,
    string CurrentTurnPlayerId,
    IReadOnlyList<GamePlayerDto> Players,
    IReadOnlyList<FinishEntryDto> FinishOrder,
    bool IsGameOver);

public sealed record GamePlayerDto(
    string PlayerId,
    string DisplayName,
    int CardCount,
    bool HasPassed,
    bool HasFinished);

public sealed record FinishEntryDto(string PlayerId, int Place);
