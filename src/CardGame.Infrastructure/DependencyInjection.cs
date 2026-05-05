using CardGame.Application.Common;
using CardGame.Application.Games.PassTurn;
using CardGame.Application.Games.PlayCards;
using CardGame.Application.Rooms;
using CardGame.Application.Rooms.CreateRoom;
using CardGame.Application.Rooms.JoinRoom;
using CardGame.Application.Rooms.LeaveRoom;
using CardGame.Application.Rooms.StartGame;
using CardGame.Infrastructure.Persistence;
using CardGame.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CardGame.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryRoomRepository>();
        services.AddSingleton<IRoomRepository>(sp => sp.GetRequiredService<InMemoryRoomRepository>());
        services.AddSingleton<IGameEngineFactory, GameEngineFactory>();
        services.AddSingleton<IRoomCodeGenerator, RoomCodeGenerator>();
        services.AddSingleton<RoomLockManager>();

        services.AddTransient<CreateRoomHandler>();
        services.AddTransient<JoinRoomHandler>();
        services.AddTransient<LeaveRoomHandler>();
        services.AddTransient<StartGameHandler>();
        services.AddTransient<PlayCardsHandler>();
        services.AddTransient<PassTurnHandler>();

        return services;
    }
}
