using Godot;
using System;
using System.Collections.Generic;
using SpaceDogFight_client.Assets.Scripts.Game;
using SpaceDogFight_client.Assets.Scripts.Game.AI;

public partial class GameManager : Node
{
   [Export] FighterControllerBase playerController;
   [Export] FighterManager fighterManager;
   [Export] private Node2D battlefield;
   [Export] private float sendRateHz = 15f;
   
   private readonly Dictionary<string, Fighter> fightersByName = new Dictionary<string, Fighter>();
   private Fighter localFighter;
   private string localPlayerName;
   
   
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

   public void StartMatch(List<string> _playerNames, string _localName)
   {
      //clean up leftovers
      foreach (var f in fightersByName.Values)   
         fighterManager.DestroyFighter(f);

      fightersByName.Clear();
      localFighter = null;

      localPlayerName = _localName;
      if (fighterManager == null || battlefield == null)
      {
         GD.PushError("[GameManager] fighterManager or battlefield not set.");
         return;
      }

      for (int i = 0; i < _playerNames.Count; i++)
      {
         string name = _playerNames[i];
         var f = fighterManager.SpawnFighter(battlefield);
         f.Name = "Fighter_" + name;
         fightersByName[name] = f;
         
         fighterManager.RandomFighterPosition(f);

         if (name == localPlayerName)
         {
            localFighter = f;
            if (playerController != null)
            {
               fighterManager.AssignPlayerFighter(playerController, f);
            }
         }
      }
   
   }
   #endregion
   

   
}
