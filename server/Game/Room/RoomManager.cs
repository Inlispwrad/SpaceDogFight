using System;
using System.Collections.Concurrent;
using SpaceDogFight.Server.Game.Net;

namespace SpaceDogFight.Server.Game.Room;

public class RoomManager(ConnectionManager _connectionManager)
{
    private readonly ConcurrentDictionary<string, Room> rooms = new();
 
    #region API
    public bool RoomExists(string _roomName) => rooms.ContainsKey(_roomName);
    public bool TryCreateRoom(string _roomName, string _password, int _capacity = 2)
    {
        return rooms.TryAdd(_roomName, new Room(_roomName, _password, Math.Clamp(_capacity, 2, 4), _connectionManager.BroadcastAsync, _connectionManager.SendAsync));   
    }

    public Room? GetRoom(string _roomName)
    {
        rooms.TryGetValue(_roomName, out var room);
        return room;
    }
    public bool TryJoinRoom(string _roomName, string _playerId, Player _player, string _password, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(_player.playerName))
        {
            error = "Player name is required.";
            return false;
        }
        
        if (!rooms.TryGetValue(_roomName, out var room))
        {
            error = $"Room({_roomName}) not found.";
            return false;
        }

        if (!room.IsPasswordValid(_password))
        {
            error = "Password is invalid.";
            return false;
        }
        
        if (!room.TryAddPlayer(_player.playerName, _player))
        {
            if (room.HasPlayer(_player.playerName))
                error = $"Player name [{_player.playerName}] already exists.";
            else if (room.PlayerCount >= room.PlayerCapacity)
                error = "Room is full.";
            else
                error = "Failed to join room.";
            return false;
        }

        return true;
    }
    public bool LeaveRoom(string _roomName, string _playerName)
    {
        if (rooms.TryGetValue(_roomName, out var room))
        {
            room.RemovePlayer(_playerName);
            Console.WriteLine($"{_playerName} leave the room. Left {room.PlayerCount} players.");
            // 如果房间没人了，就删除
            if (!room.HasAnyPlayer())
            {
                Console.WriteLine($"Since no player left remove room {_roomName}.");
                rooms.TryRemove(_roomName, out _);
            }

            return true;
        }

        return false;
    }
    public List<string> GetAllRoomNames() => rooms.Keys.ToList();
    #endregion API
}