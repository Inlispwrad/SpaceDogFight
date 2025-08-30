namespace SpaceDogFight.Server.Game.Room;

public class Room(string _roomName, string _password, int _capacity)
{
    public string RoomName { get; set; } = _roomName;
    public string Password { get; set; } = _password;
    
    private readonly HashSet<string> playerNames = new();
    private readonly object playerLock = new();

    public int PlayerCapacity { get; } = _capacity;

    public bool HasAnyPlayer()
    {
        lock (playerLock)
            return playerNames.Count > 0;
    }
    public bool HasPlayer(string _playerName)
    {
        lock(playerLock)
            return playerNames.Contains(_playerName);
    }
    public bool TryAddPlayer(string _playerName)
    {
        lock(playerLock)
            return playerNames.Count < PlayerCapacity && playerNames.Add(_playerName);
    }
    public bool RemovePlayer(string _playerName)
    {
        lock (playerLock)
            return playerNames.Remove(_playerName);
    }

    public bool IsPasswordValid(string _inputPassword)
    {
        return string.IsNullOrEmpty(Password) || Password == _inputPassword;
    }
    
    public int PlayerCount
    {
        get { lock (playerLock) return playerNames.Count; }
    }

    public string[] GetPlayers()
    {
        lock (playerLock)
            return playerNames.ToArray();
    }

}