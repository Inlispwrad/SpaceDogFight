using SpaceDogFight.Server.Game.Net;

namespace SpaceDogFight.Server.Game.Room;

public class Room(string _roomName, string _password, int _capacity, ConnectionManager.BroadcastDelegate _broadcast, ConnectionManager.SendDelegate _send)
{
    #region Definition

    public enum RoomState : byte
    {
        InLobby = 0, 
        InGame
    }
    #endregion

    
    public string RoomName { get; set; } = _roomName;
    public string Password { get; set; } = _password;
    private readonly Dictionary<string, Player> players = new();
    private readonly object playerLock = new();
    public int PlayerCapacity { get; } = _capacity;
    public RoomState State { get; private set; } = RoomState.InLobby;
    
    
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

    #endregion
    
    
    
    #region Room Logic

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
        State = RoomState.InLobby;
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
            if (players.TryGetValue(_playerName, out var player))
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