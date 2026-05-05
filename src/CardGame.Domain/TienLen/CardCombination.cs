using CardGame.Domain.Common;

namespace CardGame.Domain.TienLen;

public sealed record CardCombination
{
    public CombinationType Type { get; }
    public IReadOnlyList<Card> Cards { get; }
    public Rank HighestRank { get; }
    public Suit HighestSuit { get; }

    public CardCombination(CombinationType type, IReadOnlyList<Card> cards)
    {
        Type = type;
        Cards = cards;

        var highest = cards.Max()!;
        HighestRank = highest.Rank;
        HighestSuit = highest.Suit;
    }

    public int PairCount => Type == CombinationType.PairSequence ? Cards.Count / 2 : 0;
}
