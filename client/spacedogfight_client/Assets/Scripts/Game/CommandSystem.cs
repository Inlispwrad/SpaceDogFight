using Godot;
using System.Collections.Generic;
using SpaceDogFight.Shared.Protocols;


public partial class CommandSystem : Node   
{     
     public struct Command(string _op)                     
     {
      public string op = _op;
     }          
     Fighter fighter = null;
     Queue<Command> commandQueue = new Queue<Command>();  
     
     
     #region  godot region     
     
     public override void _Ready()
     {
      SetPhysicsProcess(true);
      GD.Print("[OS] Ready");
     }

     public override void _Process(double delta)
     {
      while (commandQueue.Count >0 && fighter != null)
      { 
       fighter.ApplyOp(commandQueue.Dequeue().op);
      }
     }

     #endregion       
     
     #region api
    
     public void Register(Fighter fighter)
     {        
      this.fighter = fighter; 
     }
     
     public void SendCommand(Command _command)
     {
     commandQueue.Enqueue(_command);
     }

     public void SendCommandNoRepeat(Command _command)
     {
      if (CheckLastCommandNotDuplicate(_command))
      {
       SendCommand(_command);
      }
     }
     public bool CheckLastCommandNotDuplicate(Command _command)
     {
      if (commandQueue.TryPeek(out var out_command))
      {
       return out_command.op != _command.op;
      }
      return true; 
     }
     #endregion
  
    
   
  
   
        
}
