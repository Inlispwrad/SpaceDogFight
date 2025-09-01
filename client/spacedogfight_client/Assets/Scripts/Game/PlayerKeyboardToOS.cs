using Godot;

public partial class PlayerKeyboardToOS : Node
{
    [Export] public CommandSystem OS;  
    [Export] public int FighterId = 1;
    [Export] public bool DebugInput = true;

    bool wPrev, aPrev, dPrev, shiftPrev, qPrev, ePrev, sPrev;

    public override void _Ready()
    {
        OS ??= GetNodeOrNull<CommandSystem>("../CommandSystem")
           ??  GetNodeOrNull<CommandSystem>("CommandSystem")
           ??  GetTree().CurrentScene?.GetNodeOrNull<CommandSystem>("CommandSystem");
        GD.Print($"[Input] bound OS={OS?.GetPath()} id={FighterId}");
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        if (OS == null) return;

        SendSwitch(Key.W, ref wPrev, FighterOperators.ForwardOn,  FighterOperators.ForwardOff);
        SendSwitch(Key.A, ref aPrev, FighterOperators.TurnLeftOn,  FighterOperators.TurnLeftOff);
        SendSwitch(Key.D, ref dPrev, FighterOperators.TurnRightOn, FighterOperators.TurnRightOff);
        bool shift = Input.IsKeyPressed(Key.Shift);
        if (shift != shiftPrev)
        {
            if (DebugInput) GD.Print($"[Input] Shift {(shift ? "Down" : "Up")} -> {(shift ? FighterOperators.ThrustOn : FighterOperators.ThrustOff)}");
            OS.Exec(FighterId, shift ? FighterOperators.ThrustOn : FighterOperators.ThrustOff);
            shiftPrev = shift;
        }

        SendPulse(Key.Q, ref qPrev, FighterOperators.ThrustLeftDash);
        SendPulse(Key.E, ref ePrev, FighterOperators.RightThrustDash);
        SendPulse(Key.S, ref sPrev, FighterOperators.ThrustBackwardDash);
    }

    void SendSwitch(Key key, ref bool prev, string onOp, string offOp)
    {
        bool cur = Input.IsKeyPressed(key);
        if (cur != prev)
        {
            if (DebugInput) GD.Print($"[Input] {key} {(cur ? "Down" : "Up")} -> {(cur ? onOp : offOp)}");
            OS.Exec(FighterId, cur ? onOp : offOp);
        }
        prev = cur;
    }

    void SendPulse(Key key, ref bool prev, string op)
    {
        bool cur = Input.IsKeyPressed(key);
        if (cur && !prev)
        {
            if (DebugInput) GD.Print($"[Input] {key} Press -> {op}");
            OS.Exec(FighterId, op);
        }
        prev = cur;
    }
}
