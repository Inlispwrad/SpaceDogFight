using Godot;
using System;
using Shared.Core.Protocols;
using SpaceDogFight_client.Assets.Scripts.Game;
using SpaceDogFight.Shared.Protocols;

public partial class Fighter : Node2D
{
    #region necessary components
    [Export]
    private CommandSystem OS;
    [Export] public Node2D Avatar;     

    #endregion
    
    #region fighter stats
    //basic stats
    private float hp, maxEnergy, energy, energyRegen;
    
    //movement stats
    private Vector2 pos, vel;
    private float angleInRadians;
    private float boostMul, boostCostPerSec, dashSpeed, dashCost;

    private float thrustAccel, maxSpeed, turnSpeed, damp;
    //input operator
    private bool op_forward, op_turnL, op_turnR, op_boost;
        
    #endregion
   
    #region godot api
    public override void _Ready()
    {
        thrustAccel = 220f;
        maxSpeed = 420f;
        turnSpeed = 3.6f;
        damp = 60f;
        maxEnergy = 100f;
        energy = 100f;
        energyRegen = 15f;
        boostMul = 2.2f;
        boostCostPerSec = 18f;
        dashSpeed = 700f;
        dashCost = 25f;
        if (OS == null || Avatar == null)
        {
         //   GD.PushError("[Fighter] OS or Avatar not assigned.");
            return;
        }
            OS.Register(this);
    }

    public override void _PhysicsProcess(double delta)
    {
      Tick((float)delta, true);
    }

   

    #endregion
    
    
    #region self logic

    private bool SpendEnergy(float energyConsume)
    {
        if(energy < energyConsume)
            return false;
        
     
        energy = MathF.Max(energy - energyConsume, 0);
        return true;
    }
    
    private void Tick(float dt, bool log)
    {
        //rotation
        int applyTurn = (op_turnR ? 1: 0) - (op_turnL ? 1 : 0);
        angleInRadians += turnSpeed * dt * applyTurn;
        
        
   
        
        //position
        float e0 = energy;
        energy = Mathf.Clamp(energy + energyRegen * dt, 0, maxEnergy);
        float accelUsed = 0f;
        if (op_forward)
        {
            var fwd = Vector2.Right.Rotated(angleInRadians);
            float accel = thrustAccel;
            if (op_boost && SpendEnergy(boostCostPerSec * dt)) accel *= boostMul;
            accelUsed = accel;
            vel += fwd * accel * dt;
        }
        else
        {
            vel = vel.MoveToward(Vector2.Zero, damp * dt);
        }
        if (vel.Length() > maxSpeed) vel = vel.LimitLength(maxSpeed);
        pos += vel * dt;
      
        //apply movement changes
        this.Position = pos;
        this.Rotation = angleInRadians;
        //GD.Print($"position:{Position},rotation:{Rotation}");
        
        if (false)
        {
            GD.Print($"[Sim] fwd={op_forward} turn={applyTurn:+0;-0} " +
                     $"E:{e0:0.0}->{energy:0.0} accel={accelUsed:0.0} " +
                     $"vel=({vel.X:0.0},{vel.Y:0.0}) speed={vel.Length():0.0} " +
                     $"pos=({pos.X:0.0},{pos.Y:0.0}) rot={Mathf.RadToDeg(angleInRadians):0.0}°");
        }
        
    }
    #endregion
    
   

    #region control api

    public CommandSystem GetOS() => OS;
    public void ApplyOp(string op)
    {
        switch (op)
        {
            case FighterOperators.ForwardOn: op_forward = true; break;
            case FighterOperators.ForwardOff: op_forward = false; break;
            case FighterOperators.TurnLeftOn: op_turnL = true; break;
            case FighterOperators.TurnLeftOff: op_turnL = false; break;
            case FighterOperators.TurnRightOn: op_turnR = true; break;
            case FighterOperators.TurnRightOff: op_turnR = false; break;
            case FighterOperators.ThrustOn: op_boost = true; break;
            case FighterOperators.ThrustOff: op_boost = false; break;
       //    case FighterOperators.ThrustLeftDash: DoDash(-Mathf.Pi / 2f); break;
       //    case FighterOperators.RightThrustDash: DoDash(Mathf.Pi / 2f); break;
       //    case FighterOperators.ThrustBackwardDash: DoDash(Mathf.Pi); break;
            default: GD.PushWarning($"[OS] Unknown op: {op}"); break;
        }
    }

    public void CorrectMovement(FighterManager _authKey, MovementState _correction)
    {
        if (_authKey == null)
            return;
        pos = new (_correction.position.x, _correction.position.y);
        vel = new (_correction.velocity.x, _correction.velocity.y);
        angleInRadians = Mathf.DegToRad(_correction.angle);
        turnSpeed = Mathf.DegToRad(_correction.angularSpeed);
        
    }

    public void CorrectStats(FighterManager _authKey, StatsState _stats)
    {
        if(_authKey == null)
            return;
        hp = _stats.hp;
        energy = _stats.energy;
        energyRegen = _stats.energyRegen;
        
    }
    
    #endregion
 
}

