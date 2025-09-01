using System.Collections.Concurrent;

namespace SpaceDogFight.Server.Game.Net;

public class PlayerManager
{
    private readonly ConcurrentDictionary<string, Player> players = new();

    public bool RegisterPlayer(string playerId)
    {
        return players.TryAdd(playerId, new Player
        {
            playerId = playerId,
            playerName = null,
            roomId = null
        });
    }

    public Player? GetPlayer(string _playerId)
    {
        players.TryGetValue(_playerId, out var player);
        return player;
    }
    public List<string> GetAllPlayerIds() => players.Keys.ToList();
}