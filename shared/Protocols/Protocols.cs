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
    public string playerName;
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
    public bool isReady;
}

[Serializable]
public class RoomState
{
    public List<PlayerRoomArgs> players = new();
}

[Serializable]
public class RequestResponse
{
    public bool success;
    public string error;
}

[Serializable]
public class ServerMessage
{
    public string message;
}