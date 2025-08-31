using Godot;
using System;

public partial class ApplicationManager : Node
{
    [Export] private Network network; 
    public override void _Ready()
    {
        base._Ready();
        
    }
}
