using CardGame.Application.Common;
using CardGame.Domain.GameEngine;
using CardGame.Domain.TienLen;

namespace CardGame.Infrastructure.Services;

public sealed class GameEngineFactory : IGameEngineFactory
{
    public IGameEngine Create(GameType gameType) => gameType switch
    {
        GameType.TienLen => new TienLenEngine(),
        _ => throw new NotSupportedException($"Game type {gameType} is not yet supported.")
    };
}
