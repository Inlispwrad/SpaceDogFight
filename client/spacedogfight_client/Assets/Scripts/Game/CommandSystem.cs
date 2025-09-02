using Godot;
using System.Collections.Generic;
using SpaceDogFight.Shared.Protocols;


public partial class CommandSystem : Node
{
    // Log every 0.25s so it doesn't spam
   // [Export] public float DebugInterval = 0.25f;
  //  [Export] public bool  DebugMovement = true;
   // [ExportGroup("Dash")]
   // [Export] public float DashDistance = 160f;   // how far a dash travels
  //  [Export] public float DashDuration = 0.08f;  // how long the dash lasts
  //  [Export] public float DashCooldown = 0.35f;  // time before next dash
 //   private readonly Dictionary<int, ControlledFighter> _fighters = new();
  //  private double _dbgTimer;
     Fighter fighter;
     #region  godot region     
    
     public override void _Ready()
     {
      SetPhysicsProcess(true);
      GD.Print("[OS] Ready");
     }
            
     #endregion
     
     #region api
    
     public void Register(Fighter fighter)
     {
      this.fighter = fighter; 
     }
     
     public void HandleCommand(string command)
     {
      fighter.ApplyOp(command);
     }
     #endregion
  
    
   
  
   
        
}
