using CardGame.Domain.Common;
using CardGame.Domain.GameEngine;

namespace CardGame.Domain.Rooms;

public sealed class Room
{
    private readonly List<Player> _players = [];

    public RoomCode Code { get; }
    public GameType GameType { get; }
    public RoomStatus Status { get; private set; }
    public IReadOnlyList<Player> Players => _players.AsReadOnly();
    public PlayerId HostPlayerId { get; private set; }
    public int MaxPlayers { get; }
    public DateTime CreatedAt { get; }
    public IGameState? ActiveGameState { get; set; }
    public int RoundNumber { get; set; }
    public PlayerId? LastRoundWinnerId { get; set; }

    public Room(RoomCode code, GameType gameType, int maxPlayers = 4)
    {
        Code = code;
        GameType = gameType;
        MaxPlayers = maxPlayers;
        Status = RoomStatus.Waiting;
        CreatedAt = DateTime.UtcNow;
    }

    public Player AddPlayer(PlayerId playerId, string displayName, string connectionId)
    {
        if (_players.Count >= MaxPlayers)
            throw new InvalidOperationException("Room is full.");

        if (Status == RoomStatus.Playing)
            throw new InvalidOperationException("Cannot join a room with a game in progress.");

        var seatIndex = _players.Count;
        var player = new Player(playerId, displayName, connectionId, seatIndex);
        _players.Add(player);

        if (_players.Count == 1)
            HostPlayerId = playerId;

        return player;
    }

    public void RemovePlayer(PlayerId playerId)
    {
        var player = _players.FirstOrDefault(p => p.Id == playerId);
        if (player is null) return;

        _players.Remove(player);

        if (_players.Count > 0 && HostPlayerId == playerId)
            HostPlayerId = _players[0].Id;
    }

    public Player? FindPlayer(PlayerId playerId) =>
        _players.FirstOrDefault(p => p.Id == playerId);

    public Player? FindPlayerByConnectionId(string connectionId) =>
        _players.FirstOrDefault(p => p.ConnectionId == connectionId);

    public void StartGame()
    {
        if (Status == RoomStatus.Playing)
            throw new InvalidOperationException("Game is already in progress.");

        if (_players.Count < 2)
            throw new InvalidOperationException("Need at least 2 players to start.");

        Status = RoomStatus.Playing;
    }

    public void EndGame()
    {
        Status = RoomStatus.Waiting;
        ActiveGameState = null;
    }

    public bool IsHost(PlayerId playerId) => HostPlayerId == playerId;
    public bool IsEmpty => _players.Count == 0;
    public bool IsFull => _players.Count >= MaxPlayers;
}
