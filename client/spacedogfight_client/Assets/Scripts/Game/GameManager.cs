using Godot;
using System;
using SpaceDogFight_client.Assets.Scripts.Game;
using SpaceDogFight_client.Assets.Scripts.Game.AI;

public partial class GameManager : Node
{
   [Export] FighterControllerBase playerController;
   [Export] FighterManager fighterManager;
   [Export] private Node2D battlefield;

   #region godot

   public override void _Ready()
   {//初始化生成逻辑
      var playerFighter =  fighterManager.SpawnFighter(battlefield);
      fighterManager.AssignPlayerFighter(playerController, playerFighter);
      for (int i = 0; i < 3; i++)
      {
         var AI =new AI_Control();
         AI.Name = "AI_" + i;
         AddChild(AI);
         var AIFighter = fighterManager.SpawnFighter(battlefield); 
         
         AIFighter.Name = "AIFighter_" + i;
         fighterManager.AssignPlayerFighter(AI, AIFighter);
         fighterManager.RandomFighterPosition(AIFighter);
      }
      
   }
   
   #endregion
   
   #region api



   #endregion
   
   
}
