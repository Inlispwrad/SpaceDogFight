using Shared.Core.DataTypes;
using SpaceDogFight.Server.Game.Core;
using SpaceDogFight.Server.Game.Net;
using SpaceDogFight.Shared.Protocols;

namespace SpaceDogFight.Server.Game.Room;

public class Room(string _roomName, string _password, int _capacity, ConnectionManager.BroadcastDelegate _broadcast, ConnectionManager.SendDelegate _send)
{
    #region Definition

    public enum RoomState : byte
    {
        InRoom = 0, 
        Countdown,
        InGame
    }
    #endregion

    
    public string RoomName { get; set; } = _roomName;
    public string Password { get; set; } = _password;
    private readonly Dictionary<string, Player> players = new();
    private readonly object playerLock = new();
    public int PlayerCapacity { get; } = _capacity;
    public RoomState State { get; private set; } = RoomState.InRoom;

    private GameManager gameManager;
    
    
    #region Message
    private readonly ConnectionManager.SendDelegate sendMsg = _send;
    public async Task BroadcastAsync(string _op, object _data, List<string>? _exception = null)
    {
        List<string> ids;

        lock (playerLock)
        {
            ids = players.Values
                .Select(p => p.playerId)
                .Where(id => _exception == null || !_exception.Contains(id))
                .ToList();
        }

        await _broadcast(ids, _op, _data);
    }

    public async Task BroadcastRoomState()
    {
        var roomState = new global::Shared.Core.DataTypes.RoomState();
        roomState.players = new();
        foreach (var player in players)
        {
            roomState.players.Add(new PlayerRoomArgs()
            {
                playerName = player.Value.playerName,
                isReady = player.Value.IsReady
            });
        }

        await BroadcastAsync(ServerMsgTypes.RoomState, roomState);
    }

    public async Task GameMessageHandler(MsgContext _ctx)
    {
        if (gameManager != null)
        {
            await gameManager.HandleMsg(_ctx);
        }
        else
        {
            Console.WriteLine($"[Server::Error] Game is not start!!! cannot handle: {_ctx.Envelope.ToJsonString()}");
            return;
        }
    }

    #endregion
    
    
    #region Room Logic
    private CancellationTokenSource countdownCts;
    private Task? countdownTask;

    public void TryStartCountdown()
    {
        lock (playerLock)
        {
            if (State != RoomState.InRoom || !ReadyCheck() || countdownTask != null)
                return;

            State = RoomState.Countdown;
            countdownCts = new CancellationTokenSource();
            countdownTask = CountdownAsync(countdownCts.Token);
        }

    }

    private async Task CountdownAsync(CancellationToken _token)
    {
        for (int i = 6; i >= 1; i--)
        {
            await BroadcastAsync(ServerMsgTypes.Message, new ServerMessage()
            {
                message = $"[color=red]Game is starting in ...... {i - 1}s[/color]"
            });

            try
            {
                await Task.Delay(1000, _token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            lock (playerLock)
            {
                if (!ReadyCheck())
                {
                    State = RoomState.InRoom;
                    countdownTask = null;
                    return;
                }
            }
        }

        State = RoomState.InGame;
        countdownTask = null;
        
        gameManager = new GameManager(this);
        await BroadcastAsync(ServerMsgTypes.GameStart, new { });
    }
    private bool ReadyCheck()
    {
        lock (playerLock)
            return players.Count > 0 && players.Values.All(_p => _p.IsReady);
    }
    private async void GameStart()
    {
        State = RoomState.InGame;
        await BroadcastAsync("GameStart", new {});
    }
    private async void GameEnd()
    {
        State = RoomState.InRoom;
        await BroadcastAsync("GameEnd", new {});
    }
    #endregion
    
    
    #region Room Connection
    public bool HasAnyPlayer()
    {
        lock (playerLock)
            return players.Count > 0;
    }
    public bool HasPlayer(string _playerName)
    {
        lock(playerLock)
            return players.ContainsKey(_playerName);
    }
    public bool TryAddPlayer(string _playerName, Player _player)
    {
        lock (playerLock)
        {
            if (players.Count < PlayerCapacity && players.TryAdd(_playerName, _player))
            {
                _player.roomId = this.RoomName;
                return true;
            }
        }

        return false;
    }
    public bool RemovePlayer(string _playerName)
    {
        lock (playerLock)
        {
            if (players.Remove(_playerName, out var player))
            {
                player.roomId = null;
                return true;
            }
            return false;
        }
    }
    public bool IsPasswordValid(string _inputPassword)
    {
        return string.IsNullOrEmpty(Password) || Password == _inputPassword;
    }
    public int PlayerCount
    {
        get { lock (playerLock) return players.Count; }
    }
    public List<Player> GetAllPlayers()
    {
        lock (playerLock)
            return players.Values.ToList();
    }
    #endregion
}