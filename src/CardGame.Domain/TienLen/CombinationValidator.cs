using CardGame.Domain.Common;

namespace CardGame.Domain.TienLen;

public static class CombinationValidator
{
    public static CardCombination? Detect(IReadOnlyList<Card> cards)
    {
        if (cards.Count == 0) return null;

        var sorted = cards.OrderBy(c => c).ToList();

        return sorted.Count switch
        {
            1 => new CardCombination(CombinationType.Single, sorted),
            2 => IsPair(sorted) ? new CardCombination(CombinationType.Pair, sorted) : null,
            3 => DetectThreeCards(sorted),
            4 => DetectFourCards(sorted),
            _ => DetectMultiCards(sorted)
        };
    }

    public static bool CanBeat(CardCombination current, CardCombination proposed)
    {
        if (IsTwoBeater(current, proposed))
            return true;

        if (current.Type != proposed.Type)
            return false;

        if (current.Cards.Count != proposed.Cards.Count)
            return false;

        return CompareHighest(proposed, current) > 0;
    }

    private static bool IsTwoBeater(CardCombination current, CardCombination proposed)
    {
        if (current.Type == CombinationType.Single && current.HighestRank == Rank.Two)
        {
            if (proposed.Type == CombinationType.Quad)
                return true;
            if (proposed.Type == CombinationType.PairSequence && proposed.PairCount >= 3)
                return true;
        }

        if (current.Type == CombinationType.Pair && current.HighestRank == Rank.Two)
        {
            if (proposed.Type == CombinationType.PairSequence && proposed.PairCount >= 4)
                return true;
        }

        if (current.Type == CombinationType.Triple && current.HighestRank == Rank.Two)
        {
            if (proposed.Type == CombinationType.PairSequence && proposed.PairCount >= 5)
                return true;
        }

        return false;
    }

    private static int CompareHighest(CardCombination a, CardCombination b)
    {
        var rankCmp = a.HighestRank.CompareTo(b.HighestRank);
        return rankCmp != 0 ? rankCmp : a.HighestSuit.CompareTo(b.HighestSuit);
    }

    private static CardCombination? DetectThreeCards(List<Card> sorted)
    {
        if (IsTriple(sorted))
            return new CardCombination(CombinationType.Triple, sorted);
        if (IsSequence(sorted))
            return new CardCombination(CombinationType.Sequence, sorted);
        return null;
    }

    private static CardCombination? DetectFourCards(List<Card> sorted)
    {
        if (IsQuad(sorted))
            return new CardCombination(CombinationType.Quad, sorted);
        if (IsSequence(sorted))
            return new CardCombination(CombinationType.Sequence, sorted);
        return null;
    }

    private static CardCombination? DetectMultiCards(List<Card> sorted)
    {
        if (IsPairSequence(sorted))
            return new CardCombination(CombinationType.PairSequence, sorted);
        if (IsSequence(sorted))
            return new CardCombination(CombinationType.Sequence, sorted);
        return null;
    }

    private static bool IsPair(IReadOnlyList<Card> cards) =>
        cards.Count == 2 && cards[0].Rank == cards[1].Rank;

    private static bool IsTriple(IReadOnlyList<Card> cards) =>
        cards.Count == 3 && cards.All(c => c.Rank == cards[0].Rank);

    private static bool IsQuad(IReadOnlyList<Card> cards) =>
        cards.Count == 4 && cards.All(c => c.Rank == cards[0].Rank);

    private static bool IsSequence(List<Card> sorted)
    {
        if (sorted.Count < 3) return false;
        if (sorted.Any(c => c.Rank == Rank.Two)) return false;

        for (var i = 1; i < sorted.Count; i++)
        {
            if (sorted[i].Rank - sorted[i - 1].Rank != 1)
                return false;
        }
        return true;
    }

    private static bool IsPairSequence(List<Card> sorted)
    {
        if (sorted.Count < 6 || sorted.Count % 2 != 0) return false;
        if (sorted.Any(c => c.Rank == Rank.Two)) return false;

        var pairs = new List<Rank>();
        for (var i = 0; i < sorted.Count; i += 2)
        {
            if (sorted[i].Rank != sorted[i + 1].Rank)
                return false;
            pairs.Add(sorted[i].Rank);
        }

        for (var i = 1; i < pairs.Count; i++)
        {
            if (pairs[i] - pairs[i - 1] != 1)
                return false;
        }
        return true;
    }
}
