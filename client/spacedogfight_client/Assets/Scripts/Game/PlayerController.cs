using System;
using Godot;

namespace SpaceDogFight_client.Assets.Scripts.Game;

public partial class PlayerController: Node
{
    #region  definition
    [Serializable]
    public class GameKey(Key defaultKey)
    {
        public Key keyCode = defaultKey;
        public bool isPressed = false;
    }

    #endregion
    
    //necessary components
    private CommandSystem os;
    
    //game key map
    private GameKey forward = new(Key.W);
    private GameKey turnL = new(Key.A);
    private GameKey turnR = new(Key.D);
    public void GetOS(Fighter _fighter)
    {
        os = _fighter.GetOS();
    }

   
    public override void _Process(double delta)
    {
        if (os == null)
            return;
        
        
        if (Input.IsKeyPressed(forward.keyCode) && forward.isPressed==false)
        {
            os.HandleCommand(FighterOperators.ForwardOn);  
            forward.isPressed = true;
        }
        else if (Input.IsKeyPressed(forward.keyCode) == false && forward.isPressed == true)
        {
           os.HandleCommand(FighterOperators.ForwardOff);  
           forward.isPressed = false;
        }
        if (Input.IsKeyPressed(turnL.keyCode) && turnL.isPressed==false)
        {
            os.HandleCommand(FighterOperators.TurnLeftOn);
            turnL.isPressed = true;
        }
        else if (Input.IsKeyPressed(turnL.keyCode) == false && turnL.isPressed == true)
        {
            os.HandleCommand(FighterOperators.TurnLeftOff);
            turnL.isPressed = false;
        }
        if (Input.IsKeyPressed(turnR.keyCode) && turnR.isPressed==false)
        {
            os.HandleCommand(FighterOperators.TurnRightOn);
            turnR.isPressed = true;
        }
        else if (Input.IsKeyPressed(turnR.keyCode) == false &&  turnR.isPressed == true)
        {
            os.HandleCommand(FighterOperators.TurnRightOff);
            turnR.isPressed = false;
        }
    }
}