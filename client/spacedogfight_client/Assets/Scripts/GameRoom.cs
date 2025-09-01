using Godot;
using System;
using System.Collections.Generic;
using Shared.Core.DataTypes;
using SpaceDogFight.Shared.Protocols;

public partial class GameRoom : Panel
{
    private string localPlayerName;

    [Export] private Network network;
    [Export] private Label roomNameLabel;
    
    [Export] private RichTextLabel chatBoard;
    [Export] private LineEdit chatInput;
    [Export] private Button sendChatBtn;

    [Export] private VBoxContainer showcase;
    [Export] private PackedScene playerShowcasePrefab;
    private Dictionary<string, PlayerShowcase> playerShowcases = new();

    [Export] private Button readyBtn;
    [Export] private Button cancelBtn;
    [Export] private Button exitBtn;
    
    public Action LeaveRoom;
    
    public override void _Ready()
    {
        Network.EventHandler_ReceivedChatMessage += ReceiveChat;
        Network.EventHandler_ReceivedRoomState += ReceiveRoomState;
        Network.EventHandler_ReceivedServerMessage += ReceiveServerMessage;

        sendChatBtn.Pressed += SendChat;
        readyBtn.Pressed += Ready;
        cancelBtn.Pressed += CancelReady;
        exitBtn.Pressed += LeaveRoom;

        foreach (var child in showcase.GetChildren())
        {
            showcase.RemoveChild(child);
        }
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        network.SendJson(Msg.Wrap(ClientMsgTypes.LeaveRoom, new{}).ToJsonString());
    }

    #region Hanlde Msg
    private void ReceiveServerMessage(ServerMessage _serverMsg)
    {
        chatBoard.Text += $"\n[server] => {_serverMsg.message}";
    }
    private void ReceiveChat(ChatMessageArgs _chatMsgs)
    {
        chatBoard.Text += $"\n[{_chatMsgs.playerName}]:{_chatMsgs.message}";
    }
    private void ReceiveRoomState(RoomState _roomState)
    {
        HashSet<string> removePlayers = new();
        foreach (var pair in playerShowcases)
        {
            removePlayers.Add(pair.Key);
        }
        foreach (var playerRoomState in _roomState.players)
        {
            if (playerRoomState == null) continue;
            
            if (playerShowcases.TryGetValue(playerRoomState.playerName, out var playerShowcase) == false)
            {
                playerShowcase = playerShowcasePrefab.Instantiate<PlayerShowcase>();
                playerShowcase.Name = "Showcase_" + playerRoomState.playerName;
                playerShowcases[playerRoomState.playerName] = playerShowcase;
                showcase.AddChild(playerShowcase);
            }
            playerShowcase.SetPlayerName(playerRoomState.playerName);
            playerShowcase.SetReady(playerRoomState.isReady);
            removePlayers.Remove(playerRoomState.playerName);
        }

        foreach (var playerNameKey in removePlayers)
        {
            showcase.RemoveChild(playerShowcases[playerNameKey]);
            playerShowcases.Remove(playerNameKey);
        }
    }

    private void SendChat()
    {
        if (string.IsNullOrWhiteSpace(chatInput.Text) == false)
        {
            Network.SendChatMessage.Invoke(localPlayerName, chatInput.Text);
            chatInput.Text = "";
        }
    }
    private void Ready()
    {
        network.SendJson(Msg.Wrap(ClientMsgTypes.Ready, new{}).ToJsonString());
    }

    private void CancelReady()
    {
        network.SendJson(Msg.Wrap(ClientMsgTypes.CancelReady, new{}).ToJsonString());
    }
    #endregion

    public void SetRoomName(string _roomName)
    {
        roomNameLabel.Text = $"Game Room: {_roomName}";
    }
    public void SetPlayerName(string _playerName)
    {
        localPlayerName = _playerName;
    }
    public void CleanUp()
    {
        chatBoard.Text = "";
        foreach (var showcasePair in playerShowcases)
        {
            if (showcasePair.Value != null)
                showcase.RemoveChild(showcasePair.Value);
        }
        playerShowcases.Clear();
    }
}
