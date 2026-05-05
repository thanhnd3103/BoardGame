using CardGame.Domain.GameEngine;

namespace CardGame.Application.Common;

public interface IGameEngineFactory
{
    IGameEngine Create(GameType gameType);
}
