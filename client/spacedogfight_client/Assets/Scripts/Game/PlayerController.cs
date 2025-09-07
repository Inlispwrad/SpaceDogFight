using System;
using Godot;

namespace SpaceDogFight_client.Assets.Scripts.Game;

public partial class PlayerController: FighterControllerBase
{
    #region  definition
    [Serializable]
    public class GameKey(Key defaultKey)
    {
        public Key keyCode = defaultKey;
        public bool isPressed = false;
    }

    #endregion
    //game key map
    private GameKey forward = new(Key.W);
    private GameKey turnL = new(Key.A);
    private GameKey turnR = new(Key.D);
    protected override void Decide()
    {
        if (Input.IsKeyPressed(forward.keyCode) && forward.isPressed==false)
        {
            os.SendCommand(new (FighterOperators.ForwardOn));  
            forward.isPressed = true;
        }
        else if (Input.IsKeyPressed(forward.keyCode) == false && forward.isPressed == true)
        {
            os.SendCommand(new (FighterOperators.ForwardOff));  
            forward.isPressed = false;
        }
        if (Input.IsKeyPressed(turnL.keyCode) && turnL.isPressed==false)
        {
            os.SendCommand(new (FighterOperators.TurnLeftOn));
            turnL.isPressed = true;
        }
        else if (Input.IsKeyPressed(turnL.keyCode) == false && turnL.isPressed == true)
        {
            os.SendCommand(new (FighterOperators.TurnLeftOff));
            turnL.isPressed = false;
        }
        if (Input.IsKeyPressed(turnR.keyCode) && turnR.isPressed==false)
        {
            os.SendCommand(new (FighterOperators.TurnRightOn));
            turnR.isPressed = true;
        }
        else if (Input.IsKeyPressed(turnR.keyCode) == false &&  turnR.isPressed == true)
        {
            os.SendCommand(new (FighterOperators.TurnRightOff));                                    
            turnR.isPressed = false;
        }
        
        
    }
        
}