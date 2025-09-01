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
        if (OS == null || Avatar == null) return;
        OS.Register(Id, Avatar);
    }
}
