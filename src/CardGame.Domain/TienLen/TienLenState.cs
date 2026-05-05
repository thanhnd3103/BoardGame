using CardGame.Domain.Common;
using CardGame.Domain.GameEngine;

namespace CardGame.Domain.TienLen;

public sealed class TienLenState : IGameState
{
    public Dictionary<PlayerId, List<Card>> PlayerHands { get; init; } = [];
    public List<PlayerId> TurnOrder { get; init; } = [];
    public int CurrentPlayerIndex { get; set; }
    public CardCombination? CurrentTrickCombination { get; set; }
    public PlayerId? CurrentTrickLeader { get; set; }
    public HashSet<PlayerId> PassedPlayers { get; init; } = [];
    public List<(PlayerId PlayerId, int Place)> FinishOrder { get; init; } = [];
    public bool IsFirstRoundOfRoom { get; init; }
    public Card? RequiredFirstCard { get; init; }
    public bool FirstCardPlayed { get; set; }
    public Dictionary<PlayerId, string> PlayerNames { get; init; } = [];

    public PlayerId CurrentPlayerId => TurnOrder[CurrentPlayerIndex];

    public IReadOnlySet<PlayerId> ActivePlayers =>
        TurnOrder.Where(p => !FinishOrder.Any(f => f.PlayerId == p)).ToHashSet();

    public bool IsGameOver
    {
        get
        {
            var activePlayers = ActivePlayers;
            return activePlayers.Count <= 1;
        }
    }

    public TienLenState Clone()
    {
        return new TienLenState
        {
            PlayerHands = PlayerHands.ToDictionary(kv => kv.Key, kv => new List<Card>(kv.Value)),
            TurnOrder = new List<PlayerId>(TurnOrder),
            CurrentPlayerIndex = CurrentPlayerIndex,
            CurrentTrickCombination = CurrentTrickCombination,
            CurrentTrickLeader = CurrentTrickLeader,
            PassedPlayers = new HashSet<PlayerId>(PassedPlayers),
            FinishOrder = new List<(PlayerId, int)>(FinishOrder),
            IsFirstRoundOfRoom = IsFirstRoundOfRoom,
            RequiredFirstCard = RequiredFirstCard,
            FirstCardPlayed = FirstCardPlayed,
            PlayerNames = new Dictionary<PlayerId, string>(PlayerNames)
        };
    }

    public void AdvanceToNextActivePlayer()
    {
        var active = ActivePlayers;
        if (active.Count == 0) return;

        do
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % TurnOrder.Count;
        } while (!active.Contains(TurnOrder[CurrentPlayerIndex]) ||
                 PassedPlayers.Contains(TurnOrder[CurrentPlayerIndex]));
    }

    public void StartNewTrick(PlayerId leadPlayer)
    {
        CurrentTrickCombination = null;
        CurrentTrickLeader = null;
        PassedPlayers.Clear();
        CurrentPlayerIndex = TurnOrder.IndexOf(leadPlayer);
    }
}
