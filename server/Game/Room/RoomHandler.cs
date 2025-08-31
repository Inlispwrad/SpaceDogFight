using Shared.Core.DataTypes;
using SpaceDogFight.Server.Game.Net;
using SpaceDogFight.Shared.Protocols;

namespace SpaceDogFight.Server.Game.Room;

public class RoomHandler(RoomManager _roomManager, ConnectionManager _connectionManager)
{
    public async Task HandleAsync(MsgContext _ctx)
    {
        switch (_ctx.Envelope.op)
        {
            case ClientMsgTypes.LeaveRoom: 
                await LeaveRoom(_ctx); 
                break;
        }
    }
    
    private async Task Chat(MsgContext _ctx)
    {
        var player = _connectionManager.playerManager.GetPlayer(_ctx.PlayerId);
        var roomId = player?.roomId;
        if (player == null || string.IsNullOrEmpty(roomId))
        {
            Console.WriteLine($"[Chat] player({_ctx.PlayerId}) not in any room.");
            return;
        }

        var room = _roomManager.GetRoom(roomId);
        if (room == null)
        {
            Console.WriteLine($"[Chat] room({roomId}) not found.");
            return;
        }

        var chat = Msg.DataAs<ChatMessageArgs>(_ctx.Envelope);
        await room.BroadcastAsync(ServerMsgTypes.Chat, new
        {
            playerName = player.playerName,
            message = chat.message
        });
    }

    private async Task LeaveRoom(MsgContext _ctx)
    {
        var player = _connectionManager.playerManager.GetPlayer(_ctx.PlayerId);
        var roomId = player?.roomId;
        if (player == null || string.IsNullOrEmpty(player.roomId))
        {
            Console.WriteLine($"[LeaveRoom] player({_ctx.PlayerId}) not in any room.");
            return;
        }

        var room = _roomManager.GetRoom(roomId);
        if (room == null)
        {
            Console.WriteLine($"[LeaveRoom] room({player.roomId}) not found.");
            return;
        }
        
        var success = _roomManager.LeaveRoom(room.RoomName, player.playerName); 

        if (success)
        {
            await _connectionManager.SendAsync(player.playerId, ServerMsgTypes.LeavedRoom, new { });
            if (_roomManager.GetRoom(roomId) != null)
            {
                await room.BroadcastAsync(ServerMsgTypes.LeavedRoom, new { playerId = _ctx.PlayerId },
                    new List<string> { _ctx.PlayerId });
            }
        }
    }
}