using CardGame.Domain.Common;
using CardGame.Domain.GameEngine;
using CardGame.Domain.Rooms;
using CardGame.Domain.TienLen;

namespace CardGame.Domain.Tests.TienLen;

public class TienLenEngineTests
{
    private readonly TienLenEngine _engine = new();

    private static List<Player> CreatePlayers(int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => new Player(PlayerId.New(), $"Player{i + 1}", $"conn{i}", i))
            .ToList();
    }

    [Fact]
    public void CreateInitialState_Deals_13_Cards_Each_For_4_Players()
    {
        var players = CreatePlayers(4);
        var (state, _) = _engine.CreateInitialState(players, true, null);

        var tienLenState = (TienLenState)state;
        foreach (var player in players)
            Assert.Equal(13, tienLenState.PlayerHands[player.Id].Count);
    }

    [Fact]
    public void CreateInitialState_Deals_13_Cards_Each_For_2_Players()
    {
        var players = CreatePlayers(2);
        var (state, _) = _engine.CreateInitialState(players, true, null);

        var tienLenState = (TienLenState)state;
        foreach (var player in players)
            Assert.Equal(13, tienLenState.PlayerHands[player.Id].Count);
    }

    [Fact]
    public void CreateInitialState_First_Player_Has_Smallest_Card()
    {
        var players = CreatePlayers(4);
        var (state, _) = _engine.CreateInitialState(players, true, null);

        var tienLenState = (TienLenState)state;
        var currentPlayer = tienLenState.CurrentPlayerId;

        var allCards = tienLenState.PlayerHands.SelectMany(kv => kv.Value).ToList();
        var smallest = allCards.Min()!;

        Assert.Contains(smallest, tienLenState.PlayerHands[currentPlayer]);
    }

    [Fact]
    public void CreateInitialState_NonFirst_Round_Uses_Previous_Winner()
    {
        var players = CreatePlayers(4);
        var winnerId = players[2].Id;
        var (state, _) = _engine.CreateInitialState(players, false, winnerId);

        var tienLenState = (TienLenState)state;
        Assert.Equal(winnerId, tienLenState.CurrentPlayerId);
    }

    [Fact]
    public void ProcessMove_PlayCards_Valid_Single()
    {
        var players = CreatePlayers(4);
        var (state, _) = _engine.CreateInitialState(players, false, players[0].Id);

        var tienLenState = (TienLenState)state;
        var hand = tienLenState.PlayerHands[players[0].Id];
        var cardToPlay = hand.OrderBy(c => c).First();

        var move = new PlayCardsMove([cardToPlay]);
        var result = _engine.ProcessMove(state, players[0].Id, move);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ProcessMove_Wrong_Turn_Fails()
    {
        var players = CreatePlayers(4);
        var (state, _) = _engine.CreateInitialState(players, false, players[0].Id);

        var tienLenState = (TienLenState)state;
        var hand = tienLenState.PlayerHands[players[1].Id];
        var cardToPlay = hand.First();

        var move = new PlayCardsMove([cardToPlay]);
        var result = _engine.ProcessMove(state, players[1].Id, move);

        Assert.False(result.IsValid);
        Assert.Contains("not your turn", result.ErrorMessage!);
    }

    [Fact]
    public void ProcessMove_Pass_On_Empty_Trick_Fails()
    {
        var players = CreatePlayers(4);
        var (state, _) = _engine.CreateInitialState(players, false, players[0].Id);

        var result = _engine.ProcessMove(state, players[0].Id, PassMove.Instance);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ProcessMove_Pass_After_Play_Succeeds()
    {
        var players = CreatePlayers(2);
        var (state, _) = _engine.CreateInitialState(players, false, players[0].Id);

        var tienLenState = (TienLenState)state;
        var hand = tienLenState.PlayerHands[players[0].Id];
        var card = hand.OrderBy(c => c).First();

        var playResult = _engine.ProcessMove(state, players[0].Id, new PlayCardsMove([card]));
        Assert.True(playResult.IsValid);

        var passResult = _engine.ProcessMove(playResult.NewState!, players[1].Id, PassMove.Instance);
        Assert.True(passResult.IsValid);
    }

    [Fact]
    public void Huong_Soai_When_All_Pass()
    {
        var players = CreatePlayers(2);
        var (state, _) = _engine.CreateInitialState(players, false, players[0].Id);

        var tienLenState = (TienLenState)state;
        var hand0 = tienLenState.PlayerHands[players[0].Id];
        var card = hand0.OrderBy(c => c).First();

        var playResult = _engine.ProcessMove(state, players[0].Id, new PlayCardsMove([card]));
        Assert.True(playResult.IsValid);

        // Player 1 passes -> all others have passed -> hưởng soái for player 0
        var passResult = _engine.ProcessMove(playResult.NewState!, players[1].Id, PassMove.Instance);
        Assert.True(passResult.IsValid);

        var newState = (TienLenState)passResult.NewState!;
        // Player 0 should lead the new trick
        Assert.Equal(players[0].Id, newState.CurrentPlayerId);
        Assert.Null(newState.CurrentTrickCombination);

        // Verify TrickWon and NewTrick events
        Assert.Contains(passResult.Events, e => e is TrickWonEvent tw && tw.WinnerId == players[0].Id);
        Assert.Contains(passResult.Events, e => e is NewTrickEvent nt && nt.LeadPlayerId == players[0].Id);
    }

    [Fact]
    public void First_Round_Requires_Starting_Card()
    {
        var players = CreatePlayers(4);
        var (state, _) = _engine.CreateInitialState(players, true, null);

        var tienLenState = (TienLenState)state;
        var currentPlayer = tienLenState.CurrentPlayerId;
        var hand = tienLenState.PlayerHands[currentPlayer];

        // Try to play a card that isn't the required first card
        var requiredCard = tienLenState.RequiredFirstCard!;
        var otherCard = hand.First(c => c != requiredCard);

        var result = _engine.ProcessMove(state, currentPlayer, new PlayCardsMove([otherCard]));
        Assert.False(result.IsValid);
        Assert.Contains("First play must include", result.ErrorMessage!);
    }

    [Fact]
    public void First_Round_Succeeds_With_Starting_Card()
    {
        var players = CreatePlayers(4);
        var (state, _) = _engine.CreateInitialState(players, true, null);

        var tienLenState = (TienLenState)state;
        var currentPlayer = tienLenState.CurrentPlayerId;
        var requiredCard = tienLenState.RequiredFirstCard!;

        var result = _engine.ProcessMove(state, currentPlayer, new PlayCardsMove([requiredCard]));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void IsGameOver_Returns_True_When_One_Player_Left()
    {
        var players = CreatePlayers(2);
        var (state, _) = _engine.CreateInitialState(players, false, players[0].Id);

        var tienLenState = (TienLenState)state;

        // Simulate player 0 finishing
        tienLenState.PlayerHands[players[0].Id].Clear();
        tienLenState.FinishOrder.Add((players[0].Id, 1));

        Assert.True(_engine.IsGameOver(tienLenState));
    }

    [Fact]
    public void GetResult_Returns_Rankings()
    {
        var players = CreatePlayers(4);
        var (state, _) = _engine.CreateInitialState(players, false, players[0].Id);

        var tienLenState = (TienLenState)state;
        tienLenState.FinishOrder.Add((players[0].Id, 1));
        tienLenState.FinishOrder.Add((players[2].Id, 2));

        var result = _engine.GetResult(tienLenState);

        Assert.Equal(4, result.Rankings.Count);
        Assert.Equal(players[0].Id, result.Rankings[0].PlayerId);
        Assert.Equal(1, result.Rankings[0].Place);
    }

    [Fact]
    public void GetPlayerView_Hides_Other_Players_Cards()
    {
        var players = CreatePlayers(4);
        var (state, _) = _engine.CreateInitialState(players, false, players[0].Id);

        var view = _engine.GetPlayerView(state, players[0].Id);

        Assert.Equal(13, view.Hand.Count);
        Assert.Equal(4, view.Players.Count);

        // Other players should show card counts but not cards
        var otherPlayer = view.Players.First(p => p.Id != players[0].Id);
        Assert.Equal(13, otherPlayer.CardCount);
    }
}
