using Godot;
using System;

public partial class Fighter : Node
{
    [Export]
    private CommandSystem OS;
    [Export] public Node2D Avatar;     
    [Export] public int Id = 1; 
    public override void _Ready()
    {
        if (OS == null) { GD.PushError("[Fighter] OS not assigned"); return; }
        if (Avatar == null) { GD.PushError("[Fighter] Avatar not assigned"); return; }
       // OS.Register(Id, Avatar);       //let os handle this ship
    }
}
