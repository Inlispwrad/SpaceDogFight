
using SpaceDogFight.Shared.Protocols;

namespace SpaceDogFight.Server.Game.Net;

public class Player
{
    public string playerId;
    public string playerName;
    public string roomId;
    
    // In Lobby Status
    public bool IsReady { get; set; } = false;
    
    // In Game Status
    public FighterState fighterState;

}