using CardGame.Domain.Rooms;

namespace CardGame.Application.Common;

public interface IRoomCodeGenerator
{
    RoomCode Generate();
}
