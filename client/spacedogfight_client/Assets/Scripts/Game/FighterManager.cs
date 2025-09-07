using System.Collections.Generic;
using Godot;
using SpaceDogFight.Shared.Protocols;

namespace SpaceDogFight_client.Assets.Scripts.Game;

public partial class FighterManager: Node
{
    [Export] private PackedScene fighterPrefab;
    private List<Fighter> activeFighters = new List<Fighter>();
    public Fighter SpawnFighter(Node2D _spawnEnvironment)
    {
        var fighter = fighterPrefab.Instantiate<Fighter>();
        _spawnEnvironment.AddChild(fighter);
        
        activeFighters.Add(fighter);
        return fighter;
    }

    public void DestroyFighter(Fighter _fighter)
    {
        _fighter.GetParent()?.RemoveChild(_fighter);
        _fighter.QueueFree();
        activeFighters.Remove(_fighter);
    }
    public void AssignPlayerFighter(FighterControllerBase _fighterControllerBase, Fighter fighter)
    {
        _fighterControllerBase.GetOS(fighter);
        
    }

    public void RandomFighterPosition(Fighter _fighter)
    {
        _fighter.CorrectMovement( this,  new MovementState(){
            position = new () {x = GD.Randf() * 400 - 200, y = GD.Randf() * 200 - 100 },
            velocity = new(),
            angle = GD.Randf() * 360
        }
        );
    }
}
