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

    public static bool HasValidMove(IReadOnlyList<Card> hand, CardCombination current)
    {
        // Two-beater special rules: quads or long pair-sequences beat a Two
        if (current.HighestRank == Rank.Two)
        {
            switch (current.Type)
            {
                case CombinationType.Single:
                    if (hand.GroupBy(c => c.Rank).Any(g => g.Count() >= 4)) return true;
                    if (HasPairSequenceOfAtLeast(hand, 3)) return true;
                    break;
                case CombinationType.Pair:
                    if (HasPairSequenceOfAtLeast(hand, 4)) return true;
                    break;
                case CombinationType.Triple:
                    if (HasPairSequenceOfAtLeast(hand, 5)) return true;
                    break;
            }
        }

        return current.Type switch
        {
            CombinationType.Single => AnyBeatingSingle(hand, current),
            CombinationType.Pair => AnyBeatingPair(hand, current),
            CombinationType.Triple => AnyBeatingTriple(hand, current),
            CombinationType.Quad => AnyBeatingQuad(hand, current),
            CombinationType.Sequence => AnyBeatingSequence(hand, current),
            CombinationType.PairSequence => AnyBeatingPairSequence(hand, current),
            _ => false
        };
    }

    private static bool AnyBeatingSingle(IReadOnlyList<Card> hand, CardCombination current) =>
        hand.Any(c => CanBeat(current, new CardCombination(CombinationType.Single, [c])));

    private static bool AnyBeatingPair(IReadOnlyList<Card> hand, CardCombination current)
    {
        foreach (var group in hand.GroupBy(c => c.Rank).Where(g => g.Count() >= 2))
        {
            var best = group.OrderByDescending(c => c.Suit).Take(2).ToList();
            if (CanBeat(current, new CardCombination(CombinationType.Pair, best))) return true;
        }
        return false;
    }

    private static bool AnyBeatingTriple(IReadOnlyList<Card> hand, CardCombination current)
    {
        foreach (var group in hand.GroupBy(c => c.Rank).Where(g => g.Count() >= 3))
        {
            var cards = group.OrderBy(c => c).Take(3).ToList();
            if (CanBeat(current, new CardCombination(CombinationType.Triple, cards))) return true;
        }
        return false;
    }

    private static bool AnyBeatingQuad(IReadOnlyList<Card> hand, CardCombination current)
    {
        foreach (var group in hand.GroupBy(c => c.Rank).Where(g => g.Count() >= 4))
        {
            var cards = group.OrderBy(c => c).Take(4).ToList();
            if (CanBeat(current, new CardCombination(CombinationType.Quad, cards))) return true;
        }
        return false;
    }

    private static bool AnyBeatingSequence(IReadOnlyList<Card> hand, CardCombination current)
    {
        var length = current.Cards.Count;
        // Best card per rank (highest suit), excluding 2s
        var byRank = hand
            .Where(c => c.Rank != Rank.Two)
            .GroupBy(c => c.Rank)
            .ToDictionary(g => g.Key, g => g.Max()!);

        var sortedRanks = byRank.Keys.OrderBy(r => r).ToList();

        for (var i = 0; i <= sortedRanks.Count - length; i++)
        {
            var window = sortedRanks.Skip(i).Take(length).ToList();
            if (!AreConsecutive(window)) continue;
            var cards = window.Select(r => byRank[r]).OrderBy(c => c).ToList();
            if (CanBeat(current, new CardCombination(CombinationType.Sequence, cards))) return true;
        }
        return false;
    }

    private static bool AnyBeatingPairSequence(IReadOnlyList<Card> hand, CardCombination current)
    {
        var pairCount = current.PairCount;
        // Best 2 cards (highest suits) per rank, excluding 2s
        var byRank = hand
            .Where(c => c.Rank != Rank.Two)
            .GroupBy(c => c.Rank)
            .Where(g => g.Count() >= 2)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.Suit).Take(2).ToList());

        var ranksWithPairs = byRank.Keys.OrderBy(r => r).ToList();

        for (var i = 0; i <= ranksWithPairs.Count - pairCount; i++)
        {
            var window = ranksWithPairs.Skip(i).Take(pairCount).ToList();
            if (!AreConsecutive(window)) continue;
            var cards = window.SelectMany(r => byRank[r]).OrderBy(c => c).ToList();
            if (CanBeat(current, new CardCombination(CombinationType.PairSequence, cards))) return true;
        }
        return false;
    }

    private static bool HasPairSequenceOfAtLeast(IReadOnlyList<Card> hand, int minPairs)
    {
        var ranksWithPairs = hand
            .Where(c => c.Rank != Rank.Two)
            .GroupBy(c => c.Rank)
            .Where(g => g.Count() >= 2)
            .Select(g => g.Key)
            .OrderBy(r => r)
            .ToList();

        if (ranksWithPairs.Count < minPairs) return false;

        var streak = 1;
        for (var i = 1; i < ranksWithPairs.Count; i++)
        {
            if (ranksWithPairs[i] - ranksWithPairs[i - 1] == 1)
            {
                if (++streak >= minPairs) return true;
            }
            else
            {
                streak = 1;
            }
        }
        return streak >= minPairs;
    }

    private static bool AreConsecutive(List<Rank> ranks)
    {
        for (var i = 1; i < ranks.Count; i++)
            if (ranks[i] - ranks[i - 1] != 1) return false;
        return true;
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
