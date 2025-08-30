using System.Collections.Concurrent;

namespace SpaceDogFight.Server.Game.Room;

public class RoomManager
{
    private readonly ConcurrentDictionary<string, Room> rooms = new();

    
    
    
    public bool RoomExists(string _roomName) => rooms.ContainsKey(_roomName);

    public bool TryCreateRoom(string _roomName, string _password, int _capacity = 2)
    {
        return rooms.TryAdd(_roomName, new Room(_roomName, _password, Math.Clamp(_capacity, 2, 4)));   
    }

    public Room? GetRoom(string _roomName)
    {
        rooms.TryGetValue(_roomName, out var room);
        return room;
    }

    public bool TryJoinRoom(string _roomName, string _playerName, string _password, out string error)
    {
        error = "";
        if (!rooms.TryGetValue(_roomName, out var room))
        {
            error = $"Room_{_roomName} not found)";
            return false;
        }

        if (!room.IsPasswordValid(_password))
        {
            error = "Password is invalid";
            return false;
        }

        if (room.PlayerCount >= room.PlayerCapacity)
        {
            error = "Room is full.";
            return false;
        }
        
        if (!room.TryAddPlayer(_playerName))
        {
            error = $"PlayerName({_playerName}) is already existed).";
            return false;
        }

        return true;
    }

    public void LeaveRoom(string _roomName, string _playerName)
    {
        if (rooms.TryGetValue(_roomName, out var room))
        {
            room.RemovePlayer(_playerName);

            // 如果房间没人了，就删除
            if (!room.HasAnyPlayer())
            {
                rooms.TryRemove(_roomName, out _);
            }
        }
    }
}