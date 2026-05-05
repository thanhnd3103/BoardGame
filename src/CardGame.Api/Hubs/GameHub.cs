using CardGame.Application.Common;
using CardGame.Application.Games.Dtos;
using CardGame.Application.Games.PassTurn;
using CardGame.Application.Games.PlayCards;
using CardGame.Application.Rooms;
using CardGame.Application.Rooms.CreateRoom;
using CardGame.Application.Rooms.Dtos;
using CardGame.Application.Rooms.JoinRoom;
using CardGame.Application.Rooms.LeaveRoom;
using CardGame.Application.Rooms.StartGame;
using CardGame.Domain.GameEngine;
using CardGame.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;

namespace CardGame.Api.Hubs;

public sealed class GameHub(
    CreateRoomHandler createRoomHandler,
    JoinRoomHandler joinRoomHandler,
    LeaveRoomHandler leaveRoomHandler,
    StartGameHandler startGameHandler,
    PlayCardsHandler playCardsHandler,
    PassTurnHandler passTurnHandler,
    IRoomRepository roomRepository,
    IGameEngineFactory engineFactory,
    RoomLockManager lockManager) : Hub<IGameHubClient>
{
    public async Task<object> CreateRoom(string displayName, string gameType)
    {
        var parsed = Enum.Parse<GameType>(gameType, ignoreCase: true);
        var command = new CreateRoomCommand(displayName, parsed, Context.ConnectionId);
        var (room, playerId) = await createRoomHandler.HandleAsync(command);

        await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomCode);

        return new { room.RoomCode, PlayerId = playerId, Room = room };
    }

    public async Task<object> JoinRoom(string roomCode, string displayName)
    {
        var command = new JoinRoomCommand(roomCode, displayName, Context.ConnectionId);
        var (room, playerId) = await joinRoomHandler.HandleAsync(command);

        await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomCode);
        await Clients.Group(room.RoomCode).RoomUpdated(room);

        return new { PlayerId = playerId, Room = room };
    }

    public async Task LeaveRoom()
    {
        var room = await roomRepository.GetByConnectionIdAsync(Context.ConnectionId);
        var roomCode = room?.Code.Value;

        var command = new LeaveRoomCommand(Context.ConnectionId);
        var updatedRoom = await leaveRoomHandler.HandleAsync(command);

        if (roomCode is not null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
            if (updatedRoom is not null)
                await Clients.Group(roomCode).RoomUpdated(updatedRoom);
        }
    }

    public async Task StartGame()
    {
        var semaphore = await GetRoomLock();
        if (semaphore is null) return;

        await semaphore.WaitAsync();
        try
        {
            var (state, events, roomCode) = await startGameHandler.HandleAsync(
                new StartGameCommand(Context.ConnectionId));

            var room = await roomRepository.GetByConnectionIdAsync(Context.ConnectionId);
            if (room is null) return;

            var engine = engineFactory.Create(room.GameType);
            await BroadcastGameState(room, engine, state);
        }
        catch (Exception ex)
        {
            await Clients.Caller.Error(ex.Message);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task PlayCards(IReadOnlyList<CardDto> cards)
    {
        var semaphore = await GetRoomLock();
        if (semaphore is null) return;

        await semaphore.WaitAsync();
        try
        {
            var command = new PlayCardsCommand(Context.ConnectionId, cards);
            var (result, roomCode) = await playCardsHandler.HandleAsync(command);

            if (!result.IsValid)
            {
                await Clients.Caller.Error(result.ErrorMessage ?? "Invalid move.");
                return;
            }

            var room = await roomRepository.GetByConnectionIdAsync(Context.ConnectionId);
            if (room is null) return;

            var engine = engineFactory.Create(room.GameType);
            var gameState = room.ActiveGameState ?? result.NewState!;

            await BroadcastGameState(room, engine, gameState);

            foreach (var evt in result.Events)
            {
                if (evt is PlayerFinishedEvent pf)
                    await Clients.Group(roomCode).PlayerFinished(pf.PlayerId.ToString(), pf.Place);
                if (evt is GameOverEvent go)
                    await Clients.Group(roomCode).GameOver(
                        go.Rankings.Select(r => new FinishEntryDto(r.PlayerId.ToString(), r.Place)).ToList());
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.Error(ex.Message);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task Pass()
    {
        var semaphore = await GetRoomLock();
        if (semaphore is null) return;

        await semaphore.WaitAsync();
        try
        {
            var command = new PassTurnCommand(Context.ConnectionId);
            var (result, roomCode) = await passTurnHandler.HandleAsync(command);

            if (!result.IsValid)
            {
                await Clients.Caller.Error(result.ErrorMessage ?? "Invalid move.");
                return;
            }

            var room = await roomRepository.GetByConnectionIdAsync(Context.ConnectionId);
            if (room is null) return;

            var engine = engineFactory.Create(room.GameType);
            var gameState = room.ActiveGameState ?? result.NewState!;

            await BroadcastGameState(room, engine, gameState);

            foreach (var evt in result.Events)
            {
                if (evt is GameOverEvent go)
                    await Clients.Group(roomCode).GameOver(
                        go.Rankings.Select(r => new FinishEntryDto(r.PlayerId.ToString(), r.Place)).ToList());
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.Error(ex.Message);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var room = await roomRepository.GetByConnectionIdAsync(Context.ConnectionId);
        if (room is not null)
        {
            var player = room.FindPlayerByConnectionId(Context.ConnectionId);
            if (player is not null)
            {
                player.Disconnect();
                await roomRepository.SaveAsync(room);
                await Clients.Group(room.Code.Value).RoomUpdated(room.ToDto());
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    private async Task BroadcastGameState(
        Domain.Rooms.Room room, IGameEngine engine, IGameState state)
    {
        foreach (var player in room.Players)
        {
            var view = engine.GetPlayerView(state, player.Id);
            var dto = view.ToDto();

            if (player.IsConnected)
                await Clients.Client(player.ConnectionId).GameStarted(dto);
        }
    }

    private async Task<SemaphoreSlim?> GetRoomLock()
    {
        var room = await roomRepository.GetByConnectionIdAsync(Context.ConnectionId);
        if (room is null)
        {
            await Clients.Caller.Error("Room not found.");
            return null;
        }
        return lockManager.GetLock(room.Code.Value);
    }
}
