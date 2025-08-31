using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using SpaceDogFight.Shared.Protocols;

namespace SpaceDogFight.Server.Game.Net;

public class ConnectionManager(MsgDispatcher _dispatcher)
{
    // Thread Safety Dictionary
    private readonly ConcurrentDictionary<string, WebSocket> sockets = new();
    public readonly PlayerManager playerManager = new();
    
    // Entry: Handle Player's entire life circle of the connection.
    public async Task HandleAsync(string _playerId, WebSocket _socket)
    {
        sockets.AddOrUpdate(_playerId, _socket, (_, _old) => _socket);
        playerManager.RegisterPlayer(_playerId);
        Console.WriteLine($"[Connection::create] player({_playerId}) connected.");

        var buffer = new byte[8 * 1024];
        var seg = new ArraySegment<byte>(buffer);

        try
        {
            while (_socket.State == WebSocketState.Open)
            {
                var result = await _socket.ReceiveAsync(seg, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"[Connection::close] player({_playerId}) disconnected.");
                    break;
                }

                if (result.MessageType != WebSocketMessageType.Text)
                {
                    continue;
                }
                
                var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"[Message::receive] player({_playerId}): {msg}");
                var env = Msg.Parse(msg);

                if (env != null && _dispatcher != null)
                {
                    var ctx = new MsgContext()
                    {
                        PlayerId = _playerId,
                        Envelope = env
                    };

                    await _dispatcher.DispatchAsync(ctx);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] player({_playerId}) got exception: {ex.Message}");
        }
        finally
        {
            try
            {
                // 只有在我们这边仍是 Open/CloseReceived 时，才礼貌性发 Close
                if (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseReceived)
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
            }
            catch { /* 忽略关闭中的异常，保持简单 */ }

            // 关键点：只在字典里仍然指向“当前这个 socket 实例”时，才移除。
            // 避免“同 pid 顶号”时，老连接把新连接的映射误删。
            if (sockets.TryGetValue(_playerId, out var current) && ReferenceEquals(current, _socket))
                sockets.TryRemove(_playerId, out _);

            _socket.Dispose();
            Console.WriteLine($"[Connection::close] player({_playerId}) disconnected.");
        }
    }
    
    public delegate Task BroadcastDelegate(List<string> _playerIds, string _op, object _data);
    public delegate Task SendDelegate(string _playerId, string _op, object _data);
    
    public async Task SendAsync(string _playerId, string _operator, Object _data)
    {
        if (sockets.TryGetValue(_playerId, out var socket) && socket.State == WebSocketState.Open)
        {
            var bytes = Msg.ToBytes(Msg.Wrap(_operator, _data));
            var seg = new ArraySegment<byte>(bytes);
            await socket.SendAsync(seg, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        else
        {
            Console.WriteLine($"[Send::fail] player({_playerId}) not connected or socket closed.");
        }
    }
    public async Task BroadcastAsync(List<string> _players, string _operator, Object _data)
    {
        var bytes = Msg.ToBytes(Msg.Wrap(_operator, _data));
        var seg = new ArraySegment<byte>(bytes);

        foreach (var playerId in _players)
        {
            if (sockets.TryGetValue(playerId, out var socket) && socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(seg, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Broadcast::error] player({playerId}) got exception: {ex.Message}");
                }
            }
        }
    }
}