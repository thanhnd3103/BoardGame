using CardGame.Domain.Common;

namespace CardGame.Domain.GameEngine;

public sealed record GameResult(IReadOnlyList<(PlayerId PlayerId, int Place)> Rankings)
{
    public PlayerId? WinnerId => Rankings.Count > 0 ? Rankings[0].PlayerId : null;
}
