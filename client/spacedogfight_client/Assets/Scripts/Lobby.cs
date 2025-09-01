using Godot;
using System;
using Shared.Core.DataTypes;
using SpaceDogFight.Shared.Protocols;

public partial class Lobby : Panel
{
    [Export] private Network network;
    [Export] private ItemList roomList;
    [Export] private RichTextLabel warningLabel;
    
    
    [Export] private LineEdit roomNameInput;
    [Export] private LineEdit passwordInput;
    [Export] private LineEdit playerNameInput;

    [Export] private Button refreshBtn;
    [Export] private Button createRoomBtn;
    [Export] private Button joinRoomBtn;

    [Export] private GameRoom room;

    private bool isRequesting = false;

    
    public override void _Ready()
    {
        this.Visible = true;
        room.Visible = false;
        
        Network.EventHandler_ReceivedRoomList += UpdateRoomList;
        Network.EventHandler_JoinRoomResponse += ReceiveJoinRoomResponse;
        Network.EventHandler_CreateRoomResponse += ReceiveCreateRoomResponse;
        
        createRoomBtn.ButtonUp += RequestCreateRoom;
        joinRoomBtn.ButtonUp += RequestJoinRoom;
        refreshBtn.Pressed += network.RequestRoomList;
        
        room.LeaveRoom = BackToLobby;
    }

    public override void _Process(double delta)
    {
        if (roomList != null && roomList.GetSelectedItems().Length > 0)
        {
            if (string.IsNullOrWhiteSpace(roomNameInput.Text))
            {
                roomNameInput.Text = roomList.GetItemText(roomList.GetSelectedItems()[0]); 
            }
        }
    }


    public void EnterToRoom()
    {
        room.Visible = true;
        this.Visible = false;
        
        room.SetRoomName(roomNameInput.Text);
        room.SetPlayerName(playerNameInput.Text);
    }

    public void BackToLobby()
    {
        this.Visible = true;
        room.Visible = false;
        isRequesting = false;
        
        network.SendJson(Msg.Wrap(ClientMsgTypes.LeaveRoom, new{}).ToJsonString());
        network.RequestRoomList();
    }


    #region MessageHandlers
    private void UpdateRoomList(RoomListArgs _roomList)
    {
        roomList.Clear();
        _roomList.roomNames.ForEach(roomName =>
        {
            roomList.AddItem(roomName);
        });
    }

    private void ReceiveCreateRoomResponse(RequestResponse _req)
    {
        if (_req.success)
        {
            EnterToRoom();
        }
        else
        {
            warningLabel.Text = $"[color=red]{_req.error}[/color]";
            isRequesting = false;
        }
    }

    private void ReceiveJoinRoomResponse(RequestResponse _req)
    {
        if (_req.success)
        {
            EnterToRoom();
        }
        else
        {
            warningLabel.Text = $"[color=red]{_req.error}[/color]";
            isRequesting = false;
        }
    }
    
    private void RequestCreateRoom()
    {
        if (isRequesting == true) return;
        
        CreateRoomArgs _args = new CreateRoomArgs()
        {
            playerName = string.IsNullOrWhiteSpace(playerNameInput.Text) ? "No-Name" : playerNameInput.Text,
            roomName = roomNameInput.Text,
            password = passwordInput.Text,
            capacity = 2
        };
        network.SendJson(Msg.Wrap(ClientMsgTypes.CreateRoom, _args).ToJsonString());
        
        warningLabel.Text = "";
        isRequesting = true;
    }
    private void RequestJoinRoom()
    {
        if (isRequesting == true) return;
        
        JoinRoomArgs _args = new JoinRoomArgs()
        {
            playerName = playerNameInput.Text,
            roomName = roomNameInput.Text,
            password = passwordInput.Text
        };
        network.SendJson(Msg.Wrap(ClientMsgTypes.JoinRoom, _args).ToJsonString());
        
        warningLabel.Text = "";
        isRequesting = true;
    }
    #endregion
}
