using CardGame.Domain.Common;

namespace CardGame.Domain.Tests.Common;

public class CardTests
{
    [Fact]
    public void Three_Of_Spades_Is_Smallest()
    {
        var smallest = new Card(Rank.Three, Suit.Spades);
        var largest = new Card(Rank.Two, Suit.Hearts);

        Assert.True(smallest < largest);
    }

    [Fact]
    public void Two_Of_Hearts_Is_Largest()
    {
        var allCards = Deck.CreateOrdered();
        var sorted = allCards.OrderBy(c => c).ToList();

        Assert.Equal(new Card(Rank.Three, Suit.Spades), sorted.First());
        Assert.Equal(new Card(Rank.Two, Suit.Hearts), sorted.Last());
    }

    [Fact]
    public void Same_Rank_Ordered_By_Suit()
    {
        var spade = new Card(Rank.Five, Suit.Spades);
        var club = new Card(Rank.Five, Suit.Clubs);
        var diamond = new Card(Rank.Five, Suit.Diamonds);
        var heart = new Card(Rank.Five, Suit.Hearts);

        Assert.True(spade < club);
        Assert.True(club < diamond);
        Assert.True(diamond < heart);
    }

    [Fact]
    public void Higher_Rank_Always_Wins_Regardless_Of_Suit()
    {
        var fourHearts = new Card(Rank.Four, Suit.Hearts);
        var fiveSpades = new Card(Rank.Five, Suit.Spades);

        Assert.True(fourHearts < fiveSpades);
    }
}
