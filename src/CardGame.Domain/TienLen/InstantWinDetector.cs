using CardGame.Domain.Common;

namespace CardGame.Domain.TienLen;

public static class InstantWinDetector
{
    public static bool IsInstantWin(IReadOnlyList<Card> hand)
    {
        return HasFourTwos(hand) || HasSixConsecutivePairs(hand) || HasDragon(hand);
    }

    private static bool HasFourTwos(IReadOnlyList<Card> hand)
    {
        return hand.Count(c => c.Rank == Rank.Two) == 4;
    }

    private static bool HasSixConsecutivePairs(IReadOnlyList<Card> hand)
    {
        var rankGroups = hand
            .GroupBy(c => c.Rank)
            .Where(g => g.Count() >= 2)
            .Select(g => g.Key)
            .Where(r => r != Rank.Two)
            .OrderBy(r => r)
            .ToList();

        if (rankGroups.Count < 6) return false;

        var consecutive = 1;
        for (var i = 1; i < rankGroups.Count; i++)
        {
            if (rankGroups[i] - rankGroups[i - 1] == 1)
            {
                consecutive++;
                if (consecutive >= 6) return true;
            }
            else
            {
                consecutive = 1;
            }
        }
        return false;
    }

    private static bool HasDragon(IReadOnlyList<Card> hand)
    {
        if (hand.Count < 13) return false;

        var ranks = hand.Select(c => c.Rank).Distinct().OrderBy(r => r).ToList();
        return ranks.Count >= 13 && Enum.GetValues<Rank>().All(r => ranks.Contains(r));
    }
}
