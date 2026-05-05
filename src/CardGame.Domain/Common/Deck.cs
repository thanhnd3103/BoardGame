namespace CardGame.Domain.Common;

public static class Deck
{
    public static List<Card> CreateShuffled(Random? random = null)
    {
        var rng = random ?? Random.Shared;
        var cards = CreateOrdered();
        Shuffle(cards, rng);
        return cards;
    }

    public static List<Card> CreateOrdered()
    {
        var cards = new List<Card>(52);
        foreach (var rank in Enum.GetValues<Rank>())
        foreach (var suit in Enum.GetValues<Suit>())
            cards.Add(new Card(rank, suit));
        return cards;
    }

    private static void Shuffle(List<Card> cards, Random rng)
    {
        for (var i = cards.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }
}
