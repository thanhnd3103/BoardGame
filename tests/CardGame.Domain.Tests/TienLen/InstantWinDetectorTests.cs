using CardGame.Domain.Common;
using CardGame.Domain.TienLen;

namespace CardGame.Domain.Tests.TienLen;

public class InstantWinDetectorTests
{
    private static Card C(Rank r, Suit s) => new(r, s);

    [Fact]
    public void Four_Twos_Is_Instant_Win()
    {
        var hand = new List<Card>
        {
            C(Rank.Two, Suit.Spades), C(Rank.Two, Suit.Clubs),
            C(Rank.Two, Suit.Diamonds), C(Rank.Two, Suit.Hearts),
            C(Rank.Three, Suit.Spades), C(Rank.Four, Suit.Spades),
            C(Rank.Five, Suit.Spades), C(Rank.Six, Suit.Spades),
            C(Rank.Seven, Suit.Spades), C(Rank.Eight, Suit.Spades),
            C(Rank.Nine, Suit.Spades), C(Rank.Ten, Suit.Spades),
            C(Rank.Jack, Suit.Spades),
        };

        Assert.True(InstantWinDetector.IsInstantWin(hand));
    }

    [Fact]
    public void Six_Consecutive_Pairs_Is_Instant_Win()
    {
        var hand = new List<Card>
        {
            C(Rank.Three, Suit.Spades), C(Rank.Three, Suit.Clubs),
            C(Rank.Four, Suit.Spades), C(Rank.Four, Suit.Clubs),
            C(Rank.Five, Suit.Spades), C(Rank.Five, Suit.Clubs),
            C(Rank.Six, Suit.Spades), C(Rank.Six, Suit.Clubs),
            C(Rank.Seven, Suit.Spades), C(Rank.Seven, Suit.Clubs),
            C(Rank.Eight, Suit.Spades), C(Rank.Eight, Suit.Clubs),
            C(Rank.Ace, Suit.Hearts),
        };

        Assert.True(InstantWinDetector.IsInstantWin(hand));
    }

    [Fact]
    public void Dragon_Is_Instant_Win()
    {
        var hand = new List<Card>
        {
            C(Rank.Three, Suit.Spades), C(Rank.Four, Suit.Spades),
            C(Rank.Five, Suit.Spades), C(Rank.Six, Suit.Spades),
            C(Rank.Seven, Suit.Spades), C(Rank.Eight, Suit.Spades),
            C(Rank.Nine, Suit.Spades), C(Rank.Ten, Suit.Spades),
            C(Rank.Jack, Suit.Spades), C(Rank.Queen, Suit.Spades),
            C(Rank.King, Suit.Spades), C(Rank.Ace, Suit.Spades),
            C(Rank.Two, Suit.Spades),
        };

        Assert.True(InstantWinDetector.IsInstantWin(hand));
    }

    [Fact]
    public void Normal_Hand_Is_Not_Instant_Win()
    {
        var hand = new List<Card>
        {
            C(Rank.Three, Suit.Spades), C(Rank.Three, Suit.Clubs),
            C(Rank.Five, Suit.Diamonds), C(Rank.Seven, Suit.Hearts),
            C(Rank.Nine, Suit.Spades), C(Rank.Jack, Suit.Clubs),
            C(Rank.King, Suit.Diamonds), C(Rank.Six, Suit.Hearts),
            C(Rank.Eight, Suit.Spades), C(Rank.Ten, Suit.Clubs),
            C(Rank.Queen, Suit.Diamonds), C(Rank.Ace, Suit.Hearts),
            C(Rank.Two, Suit.Spades),
        };

        Assert.False(InstantWinDetector.IsInstantWin(hand));
    }

    [Fact]
    public void Five_Consecutive_Pairs_Is_Not_Instant_Win()
    {
        var hand = new List<Card>
        {
            C(Rank.Three, Suit.Spades), C(Rank.Three, Suit.Clubs),
            C(Rank.Four, Suit.Spades), C(Rank.Four, Suit.Clubs),
            C(Rank.Five, Suit.Spades), C(Rank.Five, Suit.Clubs),
            C(Rank.Six, Suit.Spades), C(Rank.Six, Suit.Clubs),
            C(Rank.Seven, Suit.Spades), C(Rank.Seven, Suit.Clubs),
            C(Rank.Ace, Suit.Hearts), C(Rank.King, Suit.Hearts),
            C(Rank.Two, Suit.Spades),
        };

        Assert.False(InstantWinDetector.IsInstantWin(hand));
    }
}
