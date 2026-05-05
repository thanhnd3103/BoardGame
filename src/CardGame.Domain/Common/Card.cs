namespace CardGame.Domain.Common;

public sealed record Card(Rank Rank, Suit Suit) : IComparable<Card>
{
    public int CompareTo(Card? other)
    {
        if (other is null) return 1;
        var rankComparison = Rank.CompareTo(other.Rank);
        return rankComparison != 0 ? rankComparison : Suit.CompareTo(other.Suit);
    }

    public static bool operator <(Card left, Card right) => left.CompareTo(right) < 0;
    public static bool operator >(Card left, Card right) => left.CompareTo(right) > 0;
    public static bool operator <=(Card left, Card right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Card left, Card right) => left.CompareTo(right) >= 0;

    public override string ToString() => $"{Rank} of {Suit}";
}
