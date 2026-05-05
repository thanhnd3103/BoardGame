using CardGame.Domain.Common;

namespace CardGame.Domain.Tests.Common;

public class DeckTests
{
    [Fact]
    public void CreateOrdered_Has_52_Cards()
    {
        var deck = Deck.CreateOrdered();
        Assert.Equal(52, deck.Count);
    }

    [Fact]
    public void CreateOrdered_Has_All_Unique_Cards()
    {
        var deck = Deck.CreateOrdered();
        var distinct = deck.Distinct().Count();
        Assert.Equal(52, distinct);
    }

    [Fact]
    public void CreateShuffled_Has_52_Cards()
    {
        var deck = Deck.CreateShuffled();
        Assert.Equal(52, deck.Count);
    }

    [Fact]
    public void CreateShuffled_Is_Different_From_Ordered()
    {
        var ordered = Deck.CreateOrdered();
        var shuffled = Deck.CreateShuffled(new Random(42));

        Assert.False(ordered.SequenceEqual(shuffled));
    }
}
