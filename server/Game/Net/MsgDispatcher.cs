using SpaceDogFight.Server.Game.Room;
using SpaceDogFight.Shared.Protocols;

namespace SpaceDogFight.Server.Game.Net;

public class MsgDispatcher(RoomManagerHandler _roomManagerHandler, RoomHandler _roomHandler)
{
    public async Task DispatchAsync(MsgContext _ctx)
    {
        switch (_ctx.Envelope.op)
        {
            case ClientMsgTypes.CreateRoom: 
            case ClientMsgTypes.JoinRoom:
            case ClientMsgTypes.RequestRoomList:
                await _roomManagerHandler.HandleAsync(_ctx);
                break;
            case ClientMsgTypes.LeaveRoom:
            case ClientMsgTypes.Chat:
            case ClientMsgTypes.Ready:
            case ClientMsgTypes.CancelReady:
                await _roomHandler.HandleAsync(_ctx);
                break;
        }
    }
}