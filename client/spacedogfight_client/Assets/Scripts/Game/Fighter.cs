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
        if (OS == null || Avatar == null)
        {
            GD.PushError("[Fighter] OS or Avatar not assigned.");
            return;
        }
        OS.Register(Id, Avatar);
    }
}
