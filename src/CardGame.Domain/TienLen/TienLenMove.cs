using CardGame.Domain.Common;
using CardGame.Domain.GameEngine;

namespace CardGame.Domain.TienLen;

public sealed record PlayCardsMove(IReadOnlyList<Card> Cards) : IGameMove;
public sealed record PassMove : IGameMove
{
    public static readonly PassMove Instance = new();
}
