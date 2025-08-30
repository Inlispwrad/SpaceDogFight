


using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace SpaceDogFight.Server.Game.Net;

public class ConnectionManager
{
    // Thread Safety Dictionary
    private readonly ConcurrentDictionary<string, WebSocket> sockets = new();
    
    // Entry: Handle Player's entire life circle of the connection.
    public async Task HandleAsync(string _playerId, WebSocket _socket)
    {
        sockets[_playerId] = _socket;
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
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] player({_playerId}) got exception: {ex.Message}");
        }
        finally
        {
            if (_socket.State == WebSocketState.Open)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
                _socket.Dispose();
                Console.WriteLine($"[Connection::close] player({_playerId}) disconnected.");
            }
        }
    }
}