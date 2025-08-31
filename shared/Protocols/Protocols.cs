namespace Shared.Core.DataTypes;

// Common
[Serializable]
public class ChatMessageArgs
{
    public string playerName;
    public string message;
}

// Client 
[Serializable]
public class JoinRoomArgs
{
    public string playerName;
    public string roomName;
    public string password;
}
[Serializable]
public class CreateRoomArgs
{
    public string roomName;
    public string password;
    public int capacity;
}

// Server
[Serializable]
public class RoomListArgs
{
    public List<string> roomNames;
}

[Serializable]
public class PlayerRoomArgs
{
    public string playerName;
    public bool isReead;
}

[Serializable]
public class RoomState
{
    public List<PlayerRoomArgs> players;
}