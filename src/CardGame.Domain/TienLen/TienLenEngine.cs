using CardGame.Domain.Common;
using CardGame.Domain.GameEngine;
using CardGame.Domain.Rooms;

namespace CardGame.Domain.TienLen;

public sealed class TienLenEngine : IGameEngine
{
    public GameType GameType => GameType.TienLen;

    public (IGameState State, IReadOnlyList<GameEvent> Events) CreateInitialState(
        IReadOnlyList<Player> players, bool isFirstRound, PlayerId? previousWinnerId)
    {
        var deck = Deck.CreateShuffled();
        var hands = DealCards(deck, players);
        var events = new List<GameEvent>();

        var playerNames = players.ToDictionary(p => p.Id, p => p.DisplayName);
        var turnOrder = players.Select(p => p.Id).ToList();

        Card? requiredFirstCard = null;
        PlayerId firstPlayerId;

        if (isFirstRound)
        {
            (firstPlayerId, requiredFirstCard) = DetermineFirstPlayer(hands, turnOrder);
        }
        else if (previousWinnerId.HasValue && turnOrder.Contains(previousWinnerId.Value))
        {
            firstPlayerId = previousWinnerId.Value;
        }
        else
        {
            firstPlayerId = turnOrder[0];
        }

        var state = new TienLenState
        {
            PlayerHands = hands,
            TurnOrder = turnOrder,
            CurrentPlayerIndex = turnOrder.IndexOf(firstPlayerId),
            IsFirstRoundOfRoom = isFirstRound,
            RequiredFirstCard = requiredFirstCard,
            FirstCardPlayed = false,
            PlayerNames = playerNames
        };

        foreach (var (playerId, hand) in hands)
        {
            if (InstantWinDetector.IsInstantWin(hand))
            {
                events.Add(new InstantWinEvent(playerId, hand));
                state.FinishOrder.Add((playerId, 1));
                return (state, events);
            }
        }

        return (state, events);
    }

    public MoveResult ProcessMove(IGameState gameState, PlayerId playerId, IGameMove move)
    {
        if (gameState is not TienLenState state)
            return MoveResult.Failure("Invalid game state.");

        if (state.CurrentPlayerId != playerId)
            return MoveResult.Failure("It is not your turn.");

        return move switch
        {
            PlayCardsMove playCards => ProcessPlayCards(state, playerId, playCards),
            PassMove => ProcessPass(state, playerId),
            _ => MoveResult.Failure("Unknown move type.")
        };
    }

    public bool IsGameOver(IGameState gameState)
    {
        return gameState is TienLenState state && state.IsGameOver;
    }

    public GameResult GetResult(IGameState gameState)
    {
        if (gameState is not TienLenState state)
            return new GameResult([]);

        var rankings = new List<(PlayerId, int)>(state.FinishOrder);

        var remainingPlayers = state.TurnOrder
            .Where(p => !rankings.Any(r => r.Item1 == p))
            .ToList();

        foreach (var p in remainingPlayers)
            rankings.Add((p, rankings.Count + 1));

        return new GameResult(rankings);
    }

    public GameStateView GetPlayerView(IGameState gameState, PlayerId playerId)
    {
        var state = (TienLenState)gameState;

        var playerInfos = state.TurnOrder.Select(pid => new PlayerInfo(
            pid,
            state.PlayerNames.GetValueOrDefault(pid, "Unknown"),
            state.PlayerHands.TryGetValue(pid, out var h) ? h.Count : 0,
            state.PassedPlayers.Contains(pid),
            state.FinishOrder.Any(f => f.PlayerId == pid)
        )).ToList();

        var hand = state.PlayerHands.TryGetValue(playerId, out var cards)
            ? cards.OrderBy(c => c).ToList()
            : [];

        return new GameStateView
        {
            Hand = hand,
            PlayArea = state.CurrentTrickCombination?.Cards ?? (IReadOnlyList<Card>)[],
            CurrentTurnPlayerId = state.CurrentPlayerId,
            Players = playerInfos,
            FinishOrder = state.FinishOrder,
            IsGameOver = state.IsGameOver
        };
    }

    private MoveResult ProcessPlayCards(TienLenState state, PlayerId playerId, PlayCardsMove move)
    {
        var newState = state.Clone();
        var hand = newState.PlayerHands[playerId];
        var events = new List<GameEvent>();

        var playedCards = new List<Card>();
        foreach (var card in move.Cards)
        {
            if (!hand.Contains(card))
                return MoveResult.Failure($"You do not have the card {card}.");
            playedCards.Add(card);
        }

        var combination = CombinationValidator.Detect(playedCards);
        if (combination is null)
            return MoveResult.Failure("Invalid card combination.");

        if (!newState.FirstCardPlayed && newState.RequiredFirstCard is not null)
        {
            if (!playedCards.Contains(newState.RequiredFirstCard))
                return MoveResult.Failure($"First play must include {newState.RequiredFirstCard}.");
        }

        if (newState.CurrentTrickCombination is not null)
        {
            if (!CombinationValidator.CanBeat(newState.CurrentTrickCombination, combination))
                return MoveResult.Failure("Your cards cannot beat the current play.");
        }

        foreach (var card in playedCards)
            hand.Remove(card);

        newState.CurrentTrickCombination = combination;
        newState.CurrentTrickLeader = playerId;
        newState.FirstCardPlayed = true;
        newState.PassedPlayers.Clear();

        events.Add(new CardsPlayedEvent(playerId, playedCards));

        if (hand.Count == 0)
        {
            var place = newState.FinishOrder.Count + 1;
            newState.FinishOrder.Add((playerId, place));
            events.Add(new PlayerFinishedEvent(playerId, place));
        }

        if (newState.IsGameOver)
        {
            FinalizeGame(newState, events);
            return MoveResult.Success(newState, events);
        }

        var activePlayers = newState.ActivePlayers;
        var activeNotPassed = activePlayers.Where(p => !newState.PassedPlayers.Contains(p) && p != playerId).ToList();

        if (activeNotPassed.Count == 0 && hand.Count > 0)
        {
            newState.StartNewTrick(playerId);
            events.Add(new NewTrickEvent(playerId));
        }
        else if (activeNotPassed.Count == 0 && hand.Count == 0)
        {
            var nextLeader = FindNextActivePlayer(newState, playerId);
            if (nextLeader.HasValue)
            {
                newState.StartNewTrick(nextLeader.Value);
                events.Add(new NewTrickEvent(nextLeader.Value));
            }
        }
        else
        {
            newState.AdvanceToNextActivePlayer();
        }

        return MoveResult.Success(newState, events);
    }

    private MoveResult ProcessPass(TienLenState state, PlayerId playerId)
    {
        if (state.CurrentTrickCombination is null)
            return MoveResult.Failure("You cannot pass when you are leading the trick.");

        var newState = state.Clone();
        var events = new List<GameEvent>();

        newState.PassedPlayers.Add(playerId);
        events.Add(new PlayerPassedEvent(playerId));

        var activePlayers = newState.ActivePlayers;
        var activeNotPassed = activePlayers
            .Where(p => !newState.PassedPlayers.Contains(p))
            .ToList();

        if (activeNotPassed.Count == 1 && activeNotPassed[0] == newState.CurrentTrickLeader)
        {
            // Hưởng soái
            var leader = newState.CurrentTrickLeader!.Value;
            events.Add(new TrickWonEvent(leader));
            newState.StartNewTrick(leader);
            events.Add(new NewTrickEvent(leader));
        }
        else if (activeNotPassed.Count == 0)
        {
            var leader = newState.CurrentTrickLeader!.Value;
            if (activePlayers.Contains(leader))
            {
                events.Add(new TrickWonEvent(leader));
                newState.StartNewTrick(leader);
                events.Add(new NewTrickEvent(leader));
            }
            else
            {
                var nextPlayer = FindNextActivePlayer(newState, leader);
                if (nextPlayer.HasValue)
                {
                    newState.StartNewTrick(nextPlayer.Value);
                    events.Add(new NewTrickEvent(nextPlayer.Value));
                }
            }
        }
        else
        {
            newState.AdvanceToNextActivePlayer();
        }

        return MoveResult.Success(newState, events);
    }

    private static void FinalizeGame(TienLenState state, List<GameEvent> events)
    {
        var remaining = state.TurnOrder
            .Where(p => !state.FinishOrder.Any(f => f.PlayerId == p))
            .ToList();

        foreach (var p in remaining)
        {
            var place = state.FinishOrder.Count + 1;
            state.FinishOrder.Add((p, place));
        }

        events.Add(new GameOverEvent(state.FinishOrder));
    }

    private static PlayerId? FindNextActivePlayer(TienLenState state, PlayerId fromPlayer)
    {
        var active = state.ActivePlayers;
        if (active.Count == 0) return null;

        var startIndex = state.TurnOrder.IndexOf(fromPlayer);
        for (var i = 1; i <= state.TurnOrder.Count; i++)
        {
            var idx = (startIndex + i) % state.TurnOrder.Count;
            if (active.Contains(state.TurnOrder[idx]))
                return state.TurnOrder[idx];
        }
        return null;
    }

    private static Dictionary<PlayerId, List<Card>> DealCards(
        List<Card> deck, IReadOnlyList<Player> players)
    {
        var hands = new Dictionary<PlayerId, List<Card>>();
        foreach (var p in players)
            hands[p.Id] = [];

        var cardsPerPlayer = players.Count switch
        {
            2 => 13,
            3 => 17,
            4 => 13,
            _ => 13
        };

        for (var i = 0; i < cardsPerPlayer * players.Count && i < deck.Count; i++)
        {
            var playerIndex = i % players.Count;
            hands[players[playerIndex].Id].Add(deck[i]);
        }

        // For 3 players, give the extra card (52 - 51 = 1) to the last player
        if (players.Count == 3 && deck.Count > 51)
            hands[players[2].Id].Add(deck[51]);

        return hands;
    }

    private static (PlayerId PlayerId, Card Card) DetermineFirstPlayer(
        Dictionary<PlayerId, List<Card>> hands, List<PlayerId> turnOrder)
    {
        var allCards = hands
            .SelectMany(kv => kv.Value.Select(c => (PlayerId: kv.Key, Card: c)))
            .OrderBy(x => x.Card)
            .ToList();

        var smallest = allCards.First();
        return (smallest.PlayerId, smallest.Card);
    }
}
