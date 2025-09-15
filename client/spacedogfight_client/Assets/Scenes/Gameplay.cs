using Godot;
using System.Collections.Generic;
using SpaceDogFight.Shared.Protocols;
using SpaceDogFight_client.Assets.Scripts.Game; 
public partial class Gameplay : Node2D
{
    [Export] public GameManager GameManager;
    public override void _Ready()
    {
        GameManager ??= GetNodeOrNull<GameManager>("GameManager")
                        ?? GetNodeOrNull<GameManager>("%GameManager");//try to grab gameManager
        if (GameManager == null)
            GD.PushError("[Gameplay] GameManager not assigned/found.");
    }
    
    public void StartMatch(System.Collections.Generic.List<string> playerNames, string localPlayerName)
        => GameManager?.StartMatch(playerNames, localPlayerName);

   /* public void ApplyCommandState(CommandState cs)
        => GameManager?.ApplyCommandState(cs);

    public void ApplyFighterState(FighterState fs)
        => GameManager?.ApplyFighterState(fs);
    */
}
