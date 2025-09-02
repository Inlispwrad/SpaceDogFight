using Godot;
using System;
using SpaceDogFight_client.Assets.Scripts.Game;

public partial class GameManager : Node
{
   [Export] PlayerController _playerController;
   [Export] private PackedScene fighterPrefab;
   [Export] private Node2D battlefield;

   #region godot

   public override void _Ready()
   {//初始化生成逻辑
      var playerFighter =  SpawnFighter();
      AssignPlayerFighter(playerFighter);
   }

   #endregion
   
   #region api

   public Fighter SpawnFighter()
   {
      var fighter = fighterPrefab.Instantiate<Fighter>();
      battlefield.AddChild(fighter);
      return fighter;
   }

   public void AssignPlayerFighter(Fighter fighter)
   {
      _playerController.GetOS(fighter);
      
   }

   #endregion
   
   
}
