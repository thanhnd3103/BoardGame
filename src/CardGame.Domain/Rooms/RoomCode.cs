namespace CardGame.Domain.Rooms;

public sealed record RoomCode
{
    private const string AllowedChars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 6;

    public string Value { get; }

    public RoomCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != CodeLength)
            throw new ArgumentException($"Room code must be {CodeLength} characters.", nameof(value));
        Value = value.ToUpperInvariant();
    }

    public static RoomCode Generate(Random? random = null)
    {
        var rng = random ?? Random.Shared;
        return new RoomCode(string.Create(CodeLength, rng, (span, r) =>
        {
            for (var i = 0; i < span.Length; i++)
                span[i] = AllowedChars[r.Next(AllowedChars.Length)];
        }));
    }

    public override string ToString() => Value;
}
