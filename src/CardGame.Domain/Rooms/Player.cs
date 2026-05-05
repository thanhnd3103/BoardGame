using CardGame.Domain.Common;

namespace CardGame.Domain.Rooms;

public sealed class Player
{
    public PlayerId Id { get; }
    public string DisplayName { get; }
    public string ConnectionId { get; private set; }
    public int SeatIndex { get; }
    public bool IsConnected { get; private set; }

    public Player(PlayerId id, string displayName, string connectionId, int seatIndex)
    {
        Id = id;
        DisplayName = displayName;
        ConnectionId = connectionId;
        SeatIndex = seatIndex;
        IsConnected = true;
    }

    public void Reconnect(string newConnectionId)
    {
        ConnectionId = newConnectionId;
        IsConnected = true;
    }

    public void Disconnect()
    {
        IsConnected = false;
    }
}
