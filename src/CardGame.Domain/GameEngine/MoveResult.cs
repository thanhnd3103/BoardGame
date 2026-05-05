namespace CardGame.Domain.GameEngine;

public sealed record MoveResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public IGameState? NewState { get; init; }
    public IReadOnlyList<GameEvent> Events { get; init; } = [];

    public static MoveResult Success(IGameState newState, IReadOnlyList<GameEvent> events) =>
        new() { IsValid = true, NewState = newState, Events = events };

    public static MoveResult Failure(string error) =>
        new() { IsValid = false, ErrorMessage = error };
}
