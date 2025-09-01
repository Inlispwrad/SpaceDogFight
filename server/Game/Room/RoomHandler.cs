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
            case ClientMsgTypes.Chat:
                await Chat(_ctx); 
                break;
            case ClientMsgTypes.LeaveRoom: 
                await LeaveRoom(_ctx); 
                break;
            case ClientMsgTypes.Ready: 
                await Ready(_ctx); 
                break;
            case ClientMsgTypes.CancelReady: 
                await CancelReady(_ctx);
                break;
            default:
                await GameMsg(_ctx);
                break;
        }
    }

    private bool GetRoomAndPlayer(MsgContext _ctx, out Player _oPlayer, out Room _oRoom)
    {
        _oPlayer = _connectionManager.playerManager.GetPlayer(_ctx.PlayerId);
        var roomId = _oPlayer?.roomId;
        if (_oPlayer == null || string.IsNullOrEmpty(roomId))
        {
            Console.WriteLine($"[{_ctx.Envelope.op}] Player({_ctx.PlayerId}) not in any room.");
            _oRoom = null;
            _oPlayer = null;
            return false;
        }
        
        _oRoom = _roomManager.GetRoom(roomId);
        if (_oRoom == null)
        {
            Console.WriteLine($"[{_ctx.Envelope.op}] Room({roomId}) not found.");
            return false;
        }
        return true;
    }

    private async Task Chat(MsgContext _ctx)
    {
        if (GetRoomAndPlayer(_ctx, out Player player, out Room room))
        {
            var chat = Msg.DataAs<ChatMessageArgs>(_ctx.Envelope);
            await room.BroadcastAsync(ServerMsgTypes.Chat, new
            {
                playerName = player.playerName,
                message = chat.message
            });
        }
    }

    private async Task LeaveRoom(MsgContext _ctx)
    {
        if (GetRoomAndPlayer(_ctx, out Player player, out Room room))
        {
            string roomId = room.RoomName;
            var success = _roomManager.LeaveRoom(room.RoomName, player.playerName);
            
            if (success)
            {
                if (_roomManager.GetRoom(roomId) != null)
                {
                    await room.BroadcastRoomState();
                    await room.BroadcastAsync(ServerMsgTypes.Message, new ServerMessage() { message = $"<{player.playerName}> has left room." });
                }
            }
        }
    }

    private async Task Ready(MsgContext _ctx)
    {
        if (GetRoomAndPlayer(_ctx, out Player player, out Room room))
        {
            player.IsReady = true;
            await room.BroadcastRoomState();
            room.TryStartCountdown();
        }
    }

    private async Task CancelReady(MsgContext _ctx)
    {
        if (GetRoomAndPlayer(_ctx, out Player player, out Room room))
        {
            player.IsReady = false;
            await room.BroadcastRoomState();
        }
    }

    private async Task GameMsg(MsgContext _ctx)
    {
        if (GetRoomAndPlayer(_ctx, out Player player, out Room room))
        {
            await room.GameMessageHandler(_ctx);
        }
    }
}