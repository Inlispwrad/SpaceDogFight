using System;
using Godot;

namespace SpaceDogFight_client.Assets.Scripts.Game;

public partial class FighterControllerBase: Node
{
   
    //necessary components
    protected CommandSystem os;
    

    public void GetOS(Fighter _fighter)
    {
        os = _fighter.GetOS();
    }

    protected virtual void Decide(){}
  
    public override void _Process(double delta)
    {
        if (os == null)
            return;
        
        Decide();
       
    }
}