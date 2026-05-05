using CardGame.Application.Common;
using CardGame.Domain.Rooms;
using CardGame.Infrastructure.Persistence;

namespace CardGame.Infrastructure.Services;

public sealed class RoomCodeGenerator(InMemoryRoomRepository roomRepository) : IRoomCodeGenerator
{
    public RoomCode Generate()
    {
        RoomCode code;
        do
        {
            code = RoomCode.Generate();
        } while (roomRepository.GetByCodeAsync(code).Result is not null);

        return code;
    }
}
