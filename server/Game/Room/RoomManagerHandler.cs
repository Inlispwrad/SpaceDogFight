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
                await CreateRoom(_ctx);
                break;
            case ClientMsgTypes.JoinRoom:
                await JoinRoom(_ctx);
                break;
            case ClientMsgTypes.RequestRoomList:
                await RequestRoomList(_ctx);
                break;
            default: break;
        }
    }

    private async Task CreateRoom(MsgContext _ctx)
    {
        var createRoomArgs = Msg.DataAs<CreateRoomArgs>(_ctx.Envelope);
        var player = _connectionManager.playerManager.GetPlayer(_ctx.PlayerId);
        player.playerName = createRoomArgs.playerName;
        Console.WriteLine($"[Server] Try to create room '{createRoomArgs.roomName}'.");

        if (player == null)
        {
            await _connectionManager.SendAsync(_ctx.PlayerId, ServerMsgTypes.CreateRoom, new  RequestResponse(){ success = false, error = "Player is invalid." });
            return;
        }

        var success = _roomManager.TryCreateRoom(createRoomArgs.roomName, string.IsNullOrWhiteSpace(createRoomArgs.password) ? "" : createRoomArgs.password, createRoomArgs.capacity);

        if (!success)
        {
            Console.WriteLine($"[Server] Failed to create room.");
            await _connectionManager.SendAsync(_ctx.PlayerId, ServerMsgTypes.CreateRoom, new RequestResponse(){ success = false, error = "Room already exists." });
            return;
        }

        Console.WriteLine($"[Server] Succeed to create room.");
        Console.WriteLine($"[Server] '{player.playerName}' try to join the created room.");
        // 创建成功后自动加入房间
        var joined = _roomManager.TryJoinRoom(createRoomArgs.roomName, _ctx.PlayerId, player, createRoomArgs.password, out var joinError);
        await _connectionManager.SendAsync(_ctx.PlayerId, ServerMsgTypes.CreateRoom, new RequestResponse(){ success = joined, error = joinError });

        if (joined)
        {
            Console.WriteLine($"[Server] '{player.playerName}' Succeed to join the room.");
            var room = _roomManager.GetRoom(createRoomArgs.roomName);
            if (room != null)
            {
                await room.BroadcastRoomState();
                await room.BroadcastAsync(ServerMsgTypes.Message, new ServerMessage()
                {
                    message = $"<{player.playerName}> has joined the room."
                });
            }
            Console.WriteLine($"[Server] Finish Create Room Request.");
        }

    }
    private async Task JoinRoom(MsgContext _ctx)
    {
        try
        {
            var args = Msg.DataAs<JoinRoomArgs>(_ctx.Envelope);
            var success = _roomManager.TryJoinRoom(args.roomName, _ctx.PlayerId,
                _connectionManager.playerManager.GetPlayer(_ctx.PlayerId), args.password, out var error);

            await _connectionManager.SendAsync(_ctx.PlayerId, ServerMsgTypes.JoinRoom,
                new RequestResponse() { success = success, error = error });

            if (success)
            {
                var room = _roomManager.GetRoom(args.roomName);
                await room.BroadcastRoomState();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JoinRoom::error] <player({_ctx.PlayerId})> exception: {ex.Message}");
            await _connectionManager.SendAsync(_ctx.PlayerId, ServerMsgTypes.JoinRoom, new RequestResponse
            {
                success = false,
                error = "Internal server error."
            });
        }
    }

    private async Task RequestRoomList(MsgContext _ctx)
    {
        if (_ctx.Envelope.op == ClientMsgTypes.RequestRoomList)
        {
            await _connectionManager.SendAsync(_ctx.PlayerId, ServerMsgTypes.RoomList, new RoomListArgs()
            {
                roomNames = _roomManager.GetAllRoomNames()
            });
        }
    }
}