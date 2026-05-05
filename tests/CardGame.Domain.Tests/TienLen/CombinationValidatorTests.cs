using CardGame.Domain.Common;
using CardGame.Domain.TienLen;

namespace CardGame.Domain.Tests.TienLen;

public class CombinationValidatorTests
{
    private static Card C(Rank r, Suit s) => new(r, s);

    [Fact]
    public void Detect_Single()
    {
        var combo = CombinationValidator.Detect([C(Rank.Ace, Suit.Hearts)]);
        Assert.NotNull(combo);
        Assert.Equal(CombinationType.Single, combo.Type);
    }

    [Fact]
    public void Detect_Pair()
    {
        var combo = CombinationValidator.Detect([C(Rank.King, Suit.Spades), C(Rank.King, Suit.Hearts)]);
        Assert.NotNull(combo);
        Assert.Equal(CombinationType.Pair, combo.Type);
    }

    [Fact]
    public void Detect_Triple()
    {
        var combo = CombinationValidator.Detect([
            C(Rank.Seven, Suit.Spades), C(Rank.Seven, Suit.Clubs), C(Rank.Seven, Suit.Hearts)
        ]);
        Assert.NotNull(combo);
        Assert.Equal(CombinationType.Triple, combo.Type);
    }

    [Fact]
    public void Detect_Quad()
    {
        var combo = CombinationValidator.Detect([
            C(Rank.Jack, Suit.Spades), C(Rank.Jack, Suit.Clubs),
            C(Rank.Jack, Suit.Diamonds), C(Rank.Jack, Suit.Hearts)
        ]);
        Assert.NotNull(combo);
        Assert.Equal(CombinationType.Quad, combo.Type);
    }

    [Fact]
    public void Detect_Sequence_Three_Cards()
    {
        var combo = CombinationValidator.Detect([
            C(Rank.Three, Suit.Spades), C(Rank.Four, Suit.Clubs), C(Rank.Five, Suit.Hearts)
        ]);
        Assert.NotNull(combo);
        Assert.Equal(CombinationType.Sequence, combo.Type);
    }

    [Fact]
    public void Detect_Sequence_Rejects_Two_In_Sequence()
    {
        var combo = CombinationValidator.Detect([
            C(Rank.King, Suit.Spades), C(Rank.Ace, Suit.Clubs), C(Rank.Two, Suit.Hearts)
        ]);
        Assert.Null(combo);
    }

    [Fact]
    public void Detect_PairSequence()
    {
        var combo = CombinationValidator.Detect([
            C(Rank.Five, Suit.Spades), C(Rank.Five, Suit.Clubs),
            C(Rank.Six, Suit.Spades), C(Rank.Six, Suit.Clubs),
            C(Rank.Seven, Suit.Spades), C(Rank.Seven, Suit.Clubs)
        ]);
        Assert.NotNull(combo);
        Assert.Equal(CombinationType.PairSequence, combo.Type);
        Assert.Equal(3, combo.PairCount);
    }

    [Fact]
    public void Detect_Invalid_Two_Different_Ranks()
    {
        var combo = CombinationValidator.Detect([C(Rank.Three, Suit.Spades), C(Rank.Five, Suit.Clubs)]);
        Assert.Null(combo);
    }

    [Fact]
    public void CanBeat_Higher_Single_Beats_Lower()
    {
        var low = new CardCombination(CombinationType.Single, [C(Rank.Five, Suit.Hearts)]);
        var high = new CardCombination(CombinationType.Single, [C(Rank.Eight, Suit.Spades)]);

        Assert.True(CombinationValidator.CanBeat(low, high));
        Assert.False(CombinationValidator.CanBeat(high, low));
    }

    [Fact]
    public void CanBeat_Same_Rank_Higher_Suit_Wins()
    {
        var lower = new CardCombination(CombinationType.Single, [C(Rank.King, Suit.Spades)]);
        var higher = new CardCombination(CombinationType.Single, [C(Rank.King, Suit.Hearts)]);

        Assert.True(CombinationValidator.CanBeat(lower, higher));
    }

    [Fact]
    public void CanBeat_Quad_Beats_Single_Two()
    {
        var singleTwo = new CardCombination(CombinationType.Single, [C(Rank.Two, Suit.Hearts)]);
        var quad = new CardCombination(CombinationType.Quad, [
            C(Rank.Three, Suit.Spades), C(Rank.Three, Suit.Clubs),
            C(Rank.Three, Suit.Diamonds), C(Rank.Three, Suit.Hearts)
        ]);

        Assert.True(CombinationValidator.CanBeat(singleTwo, quad));
    }

    [Fact]
    public void CanBeat_Three_Pair_Sequence_Beats_Single_Two()
    {
        var singleTwo = new CardCombination(CombinationType.Single, [C(Rank.Two, Suit.Hearts)]);
        var pairSeq = new CardCombination(CombinationType.PairSequence, [
            C(Rank.Three, Suit.Spades), C(Rank.Three, Suit.Clubs),
            C(Rank.Four, Suit.Spades), C(Rank.Four, Suit.Clubs),
            C(Rank.Five, Suit.Spades), C(Rank.Five, Suit.Clubs)
        ]);

        Assert.True(CombinationValidator.CanBeat(singleTwo, pairSeq));
    }

    [Fact]
    public void CanBeat_Four_Pair_Sequence_Beats_Pair_Of_Twos()
    {
        var pairTwos = new CardCombination(CombinationType.Pair, [
            C(Rank.Two, Suit.Spades), C(Rank.Two, Suit.Hearts)
        ]);
        var fourPairSeq = new CardCombination(CombinationType.PairSequence, [
            C(Rank.Three, Suit.Spades), C(Rank.Three, Suit.Clubs),
            C(Rank.Four, Suit.Spades), C(Rank.Four, Suit.Clubs),
            C(Rank.Five, Suit.Spades), C(Rank.Five, Suit.Clubs),
            C(Rank.Six, Suit.Spades), C(Rank.Six, Suit.Clubs)
        ]);

        Assert.True(CombinationValidator.CanBeat(pairTwos, fourPairSeq));
    }

    [Fact]
    public void CanBeat_Cannot_Beat_Different_Type_Same_Count()
    {
        var triple = new CardCombination(CombinationType.Triple, [
            C(Rank.Five, Suit.Spades), C(Rank.Five, Suit.Clubs), C(Rank.Five, Suit.Hearts)
        ]);
        var sequence = new CardCombination(CombinationType.Sequence, [
            C(Rank.Three, Suit.Spades), C(Rank.Four, Suit.Clubs), C(Rank.Five, Suit.Hearts)
        ]);

        Assert.False(CombinationValidator.CanBeat(triple, sequence));
    }

    [Fact]
    public void CanBeat_Higher_Sequence_Beats_Lower()
    {
        var low = new CardCombination(CombinationType.Sequence, [
            C(Rank.Three, Suit.Spades), C(Rank.Four, Suit.Clubs), C(Rank.Five, Suit.Hearts)
        ]);
        var high = new CardCombination(CombinationType.Sequence, [
            C(Rank.Six, Suit.Spades), C(Rank.Seven, Suit.Clubs), C(Rank.Eight, Suit.Hearts)
        ]);

        Assert.True(CombinationValidator.CanBeat(low, high));
    }
}
