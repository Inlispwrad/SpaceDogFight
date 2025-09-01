using System.Collections.Concurrent;
using SpaceDogFight.Server.Game.Net;
using SpaceDogFight.Shared.Protocols;

namespace SpaceDogFight.Server.Game.Core;

public class GameManager
{
    private readonly ConcurrentDictionary<string, FighterState> fighterStates = new();
    private readonly Room.Room room;

    public GameManager(Room.Room _room)
    {
        room = _room;
        foreach (var player in room.GetAllPlayers())
        {
            fighterStates.TryAdd(player.playerName, new FighterState()
            {
                playerName = player.playerName,
                movement = new(),
                stats = new()
            });
        }
    }

    public async Task HandleMsg(MsgContext _ctx)
    {
        switch (_ctx.Envelope.op)
        {
            case ClientMsgTypes.FighterStats:
            case ClientMsgTypes.FighterMovement:
                await HandleFighterState(_ctx);
                break;
        }
    }

    private async Task HandleFighterState(MsgContext _ctx)
    {
        FighterState fighterStateData = Msg.DataAs<FighterState>(_ctx.Envelope);
        if (fighterStateData == null || fighterStates.ContainsKey(fighterStateData.playerName) == false)
            return;
        
        var fighterState = fighterStates[fighterStateData.playerName];
        if (fighterStateData.movement != null)
        {
            fighterState.movement =  fighterStateData.movement;
        }
        if (fighterStateData.stats != null)
        {
            fighterState.stats = fighterStateData.stats;
        }

        await BroadCastFighterState(fighterStateData, new() { fighterStateData.playerName });
    }

    private async Task BroadCastFighterState(FighterState _fighterState, List<string> _exception)
    {
        await room.BroadcastAsync(ServerMsgTypes.FighterState, _fighterState, _exception);
    }
}