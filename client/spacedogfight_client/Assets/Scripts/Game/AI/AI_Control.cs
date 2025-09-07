using System;
using System.Timers;
using Godot;

namespace SpaceDogFight_client.Assets.Scripts.Game.AI;

public partial class AI_Control:FighterControllerBase
{
    private float currentTime = 0;
    private float maxTime = 2;
    private bool _started = false;
    
    public override void _Process(double delta)
    {
        currentTime += GD.Randf() * 4;
        currentTime += (float)delta;
        base._Process(delta);
        currentTime %= maxTime;
        
    }

    protected override void Decide()
    {
        if (_started == false)
        {
            os.SendCommandNoRepeat(new (FighterOperators.ForwardOn));
            _started = true;
        }

        if (currentTime < maxTime / 4)
        {
            os.SendCommandNoRepeat(new (FighterOperators.TurnRightOn));
        }
        else if (currentTime < maxTime/4*3 )
        {
            os.SendCommandNoRepeat(new (FighterOperators.TurnRightOff));
            os.SendCommandNoRepeat(new (FighterOperators.TurnLeftOn));
        }
        else if (currentTime < maxTime)
        {
            os.SendCommandNoRepeat(new (FighterOperators.TurnLeftOff));
            
        }
    }
    
}