using Shared.Core.DataTypes;
using SpaceDogFight.Server.Game.Net;
using SpaceDogFight.Shared.Protocols;

namespace SpaceDogFight.Server.Game.Room;

public class RoomManagerHandler(RoomManager _roomManager, ConnectionManager _connectionManager)
{
    public async Task HandleAsync(MsgContext _ctx)
    {

        switch (_ctx.Envelope.op)
        {
            case ClientMsgTypes.CreateRoom: 
                
                break;
            case ClientMsgTypes.JoinRoom:
                await JoinRoom(_ctx);
                break;
            default: break;
        }
    }

    private async Task CreateRoom(MsgContext _ctx)
    {
        var args = Msg.DataAs<CreateRoomArgs>(_ctx.Envelope);
        var player = _connectionManager.playerManager.GetPlayer(_ctx.PlayerId);

        if (player == null)
        {
            await _connectionManager.SendAsync(_ctx.PlayerId, ServerMsgTypes.JoinedRoom, new { success = false, error = "Player not found." });
            return;
        }

        var success = _roomManager.TryCreateRoom(args.roomName, string.IsNullOrWhiteSpace(args.password) ? null : args.password, args.capacity);

        if (!success)
        {
            await _connectionManager.SendAsync(_ctx.PlayerId, ServerMsgTypes.JoinedRoom, new { success = false, error = "Room already exists." });
            return;
        }

        // 创建成功后自动加入房间
        var joined = _roomManager.TryJoinRoom(args.roomName, _ctx.PlayerId, player, args.password, out var joinError);

        await _connectionManager.SendAsync(_ctx.PlayerId, ServerMsgTypes.JoinedRoom, new { success = joined, error = joinError });

        if (joined)
        {
            var room = _roomManager.GetRoom(args.roomName);
            await room.BroadcastAsync(ServerMsgTypes.JoinedRoom, new { playerId = _ctx.PlayerId, playerName = player.playerName }, new List<string> { _ctx.PlayerId });
        }

    }
    private async Task JoinRoom(MsgContext _ctx)
    {
        var args = Msg.DataAs<JoinRoomArgs>(_ctx.Envelope);
        var success = _roomManager.TryJoinRoom(args.roomName, _ctx.PlayerId, _connectionManager.playerManager.GetPlayer(_ctx.PlayerId), args.password, out var error);

        await _connectionManager.SendAsync(_ctx.PlayerId, ServerMsgTypes.JoinedRoom, new { success, error });

        if (success)
        {
            var room = _roomManager.GetRoom(args.roomName);
            await room.BroadcastAsync(ServerMsgTypes.JoinedRoom, new { playerId = _ctx.PlayerId, playerName = args.playerName }, new List<string> { _ctx.PlayerId });
        }
    }

}