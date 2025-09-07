using System.Collections.Concurrent;
using Shared.Core.Protocols;
using SpaceDogFight.Server.Game.Net;
using SpaceDogFight.Shared.Protocols;

namespace SpaceDogFight.Server.Game.Core;

public class GameManager
{
    private readonly ConcurrentDictionary<string, FighterState> fighterStates = new();
    private readonly Room.Room room;
    
    // Tick
    private CancellationTokenSource tickCts;
    private Task? tickTask;

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
    
    #region Message
    public async Task HandleMsg(MsgContext _ctx)
    {
        switch (_ctx.Envelope.op)
        {
            case ClientMsgTypes.FighterCommand:
                await HandleFighterCommand(_ctx);
                break;
            case ClientMsgTypes.FighterStats:
            case ClientMsgTypes.FighterMovement:
                await HandleFighterState(_ctx);
                break;
        }
    }

    private async Task HandleFighterCommand(MsgContext _ctx)
    {
        CommandState commandStateData = Msg.DataAs<CommandState>(_ctx.Envelope);
        if (commandStateData == null) return;

        await room.BroadcastAsync(ServerMsgTypes.FighterCommand, commandStateData, [_ctx.PlayerId]);
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

        await BroadCastFighterState(fighterStateData, [_ctx.PlayerId]);
    }

    private async Task BroadCastFighterState(FighterState _fighterState, List<string> _exceptionIds = null, List<string> _exceptionNames = null)
    {
        await room.BroadcastAsync(ServerMsgTypes.FighterState, _fighterState, _exceptionIds, _exceptionNames);
    }
    #endregion
    
    #region Gameplay Loop
    //TODO:: Gameplay logic loop
    #endregion
}