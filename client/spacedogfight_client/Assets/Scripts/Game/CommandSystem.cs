using Godot;
using System.Collections.Generic;
using SpaceDogFight.Shared.Protocols;


public partial class CommandSystem : Node
{
    // Log every 0.25s so it doesn't spam
    [Export] public float DebugInterval = 0.25f;
    [Export] public bool  DebugMovement = true;
    [ExportGroup("Dash")]
    [Export] public float DashDistance = 160f;   // how far a dash travels
    [Export] public float DashDuration = 0.08f;  // how long the dash lasts
    [Export] public float DashCooldown = 0.35f;  // time before next dash
    private readonly Dictionary<int, ControlledFighter> _fighters = new();
    private double _dbgTimer;

    public override void _Ready()
    {
        SetPhysicsProcess(true);
        GD.Print("[OS] Ready");
    }

    public void Register(int id, Node2D avatar)
    {
        var f = new ControlledFighter
        {
            id = id,
            avatar = avatar,
            pos = avatar.GlobalPosition,
            rot = avatar.Rotation,
            
            thrustAccel = 220f, maxSpeed = 420f, turnSpeed = 3.6f, damp = 60f,
            maxEnergy = 100f, energy = 100f, energyRegen = 15f,
            boostMul = 2.2f, boostCostPerSec = 18f, dashSpeed = 700f, dashCost = 25f
    
        };
        _fighters[id] = f;
       
        GD.Print($"[OS] Register fighter #{id} from {avatar.GetPath()}  pos={f.pos}");
    }

    public void Exec(int id, string op)
    {
        GD.Print($"[OS] #{id} <- {op}");
        if (!_fighters.TryGetValue(id, out var f))
        {
            GD.PrintErr($"[OS] Exec DROPPED: fighter #{id} not registered yet");
            return;
        }
        f.ApplyOp(op);
    }

    public override void _PhysicsProcess(double delta)
    {
        _dbgTimer += delta;
        bool doLog = DebugMovement && _dbgTimer >= DebugInterval;

        foreach (var kv in _fighters)
            kv.Value.Tick((float)delta, doLog);

        if (doLog) _dbgTimer = 0;
    }

    // ---------------- internal per-fighter state ----------------
    private class ControlledFighter
    {
        public int id;
        public Node2D avatar;
        public Vector2 pos, vel;
        public float rot;

        // input latches
        bool forward, turnL, turnR, boost;

        // tuning
        public float thrustAccel, maxSpeed, turnSpeed, damp;
        public float maxEnergy, energy, energyRegen, boostMul, boostCostPerSec, dashSpeed, dashCost;

        public void ApplyOp(string op)
        {
            switch (op)
            {
                case FighterOperators.ForwardOn:   forward = true;  break;
                case FighterOperators.ForwardOff:  forward = false; break;
                case FighterOperators.TurnLeftOn:  turnL = true;    break;
                case FighterOperators.TurnLeftOff: turnL = false;   break;
                case FighterOperators.TurnRightOn: turnR = true;    break;
                case FighterOperators.TurnRightOff:turnR = false;   break;
                case FighterOperators.ThrustOn:    boost = true;    break;
                case FighterOperators.ThrustOff:   boost = false;   break;
                case FighterOperators.ThrustLeftDash:     DoDash(-Mathf.Pi/2f); break;
                case FighterOperators.RightThrustDash:    DoDash( Mathf.Pi/2f); break;
                case FighterOperators.ThrustBackwardDash: DoDash( Mathf.Pi    ); break;
                default: GD.PushWarning($"[OS] Unknown op: {op}"); break;
            }
        }

        public void Tick(float dt, bool log)
        {
            float e0 = energy;
            energy = Mathf.Clamp(energy + energyRegen * dt, 0, maxEnergy);

            float turn = (turnR ? 1f : 0f) - (turnL ? 1f : 0f);
            rot += turn * turnSpeed * dt;

            float accelUsed = 0f;
            if (forward)
            {
                var fwd = Vector2.Right.Rotated(rot);
                float accel = thrustAccel;
                if (boost && Spend(boostCostPerSec * dt)) accel *= boostMul;
                accelUsed = accel;
                vel += fwd * accel * dt;
            }
            else
            {
                vel = vel.MoveToward(Vector2.Zero, damp * dt);
            }

            if (vel.Length() > maxSpeed) vel = vel.LimitLength(maxSpeed);
            pos += vel * dt;

            if (avatar != null)
            {
                avatar.GlobalPosition = pos;
                avatar.Rotation = rot;
            }

            if (log)
            {
                GD.Print($"[Sim] id={id} fwd={forward} turn={turn:+0;-0} " +
                         $"E:{e0:0.0}->{energy:0.0} accel={accelUsed:0.0} " +
                         $"vel=({vel.X:0.0},{vel.Y:0.0}) speed={vel.Length():0.0} " +
                         $"pos=({pos.X:0.0},{pos.Y:0.0}) rot={Mathf.RadToDeg(rot):0.0}°");
            }
        }
        
        
        void DoDash(float localAngle)
        {
            if (!Spend(dashCost))
            {
                GD.Print($"[Dash] id={id} NOT ENOUGH ENERGY ({energy:0.0}/{dashCost})");
                return;
            }
            var dir = Vector2.Right.Rotated(rot + localAngle);
            vel += dir * dashSpeed;
            GD.Print($"[Dash] id={id} vel=({vel.X:0.0},{vel.Y:0.0}) E={energy:0.0}");
        }

        bool Spend(float a) { if (energy < a) return false; energy -= a; return true; }
    }
}
