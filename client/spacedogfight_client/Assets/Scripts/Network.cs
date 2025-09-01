using System;
using Godot;
using System.Text;
using Shared.Core.DataTypes;
using Shared.Core.Protocols;
using SpaceDogFight.Shared.Protocols;

public partial class Network : Node
{
    #region BlackScreen
    [Export] private BlackVeil blackScreen;

    private void ShowBlackScreen(string _noticeText)
    {
        if (blackScreen != null)
        {
            blackScreen.Show();
            blackScreen.SetNotice(_noticeText);
        }
    }
    #endregion
    
    
    #region WebSocket
    private WebSocketPeer _ws;
    // 按你实际端口改：如果是日志里 63623 就改成 63623；默认 Kestrel 本地是 5000
    private const string Url = "wss://57d09112cae6.ngrok-free.app/ws"; // or 63623

    private bool _connectedAnnounced = false;
    private double _heartbeat = 0;
    private double _reconnectCd = 0;
    private int _retry = 0;
    
    public override void _Ready()
    {
        SendFighterMovement = (MovementState _fighterMovement) =>
        {
            SendJson(Msg.Wrap(ClientMsgTypes.FighterMovement, _fighterMovement).ToJsonString());
        };
        SendFighterStats = (StatsState _fighterStats) =>
        {
            SendJson(Msg.Wrap(ClientMsgTypes.FighterStats, _fighterStats).ToJsonString());
        };
        
        ShowBlackScreen("Initializing Network...");
        _ws = new WebSocketPeer();
        var err = _ws.ConnectToUrl(Url);                  // 这里只是“开始连接”，不是已连上
        GD.Print($"[WS] start connect, ret={err}");
    }

    public override void _Process(double delta)
    {
        if (_ws == null) return;

        _ws.Poll();
        switch (_ws.GetReadyState())
        {
            case WebSocketPeer.State.Connecting:
                ShowBlackScreen("Connecting to server...");
                // 等待握手完成
                break;

            case WebSocketPeer.State.Open:
                if (!_connectedAnnounced)
                {
                    _connectedAnnounced = true;
                    _retry = 0; _reconnectCd = 0;
                    GD.Print("[Network] CONNECTED");
                    // 初次打个招呼（按你服务器的协议替换）
                    _ws.SendText("{\"type\":\"hello\",\"from\":\"godot\"}");
                    
                    // Init Request Hook
                    SendChatMessage = Chat;
                    
                    
                    RequestRoomList();
                    
                    ShowBlackScreen("Connected!!!");
                    blackScreen?.Close(2f);
                }

                // 收包
                while (_ws.GetAvailablePacketCount() > 0)
                {
                    var pkt = _ws.GetPacket();
                    var jsonString = Encoding.UTF8.GetString(pkt);
                    GD.Print("[Network] RECV: " + jsonString);
                    MsgDispatcher(Msg.Parse(jsonString));
                }

                // 心跳（每 15s）
                _heartbeat += delta;
                if (_heartbeat >= 15.0)
                {
                    _heartbeat = 0;
                    //_ws.SendText("{\"type\":\"ping\"}");
                    //Chat("Jin", "Hello! This is beats.");
                }
                break;

            case WebSocketPeer.State.Closing:
                // 等待关闭完成
                break;

            case WebSocketPeer.State.Closed:
                GD.PrintErr($"[Network] CONNECTION CLOSED code={_ws.GetCloseCode()} reason={_ws.GetCloseReason()}");
                TryReconnect(delta);
                break;
        }
    }

    private void TryReconnect(double delta)
    {
        _reconnectCd -= delta;
        if (_reconnectCd > 0) return;

        _retry = Mathf.Min(_retry + 1, 6);
        _reconnectCd = 0.5 * Mathf.Pow(2, _retry); // 0.5,1,2,4,8,16s
        GD.Print($"[WS] RETRY #{_retry}");

        _connectedAnnounced = false;
        _ws = new WebSocketPeer();
        _ws.ConnectToUrl(Url);
    }

    public override void _ExitTree()
    {
        _ws?.Close();
    }

    // 业务侧主动发消息
    public void SendJson(string _json)
    {
        if (_ws != null && _ws.GetReadyState() == WebSocketPeer.State.Open)
            _ws.SendText(_json);
    }
    #endregion
    
    #region Message Requester

    public static Action<string, string> SendChatMessage { get; private set; }
    public static Action<MovementState> SendFighterMovement { get; private set; }
    public static Action<StatsState> SendFighterStats { get; private set; }
    #endregion
    
    #region Message EventHandler
    // Room
    public static event Action<RoomListArgs> EventHandler_ReceivedRoomList;
    public static event Action<RoomState> EventHandler_ReceivedRoomState;
    public static event Action<RequestResponse> EventHandler_JoinRoomResponse;
    public static event Action<RequestResponse> EventHandler_CreateRoomResponse;
    // Chat
    public static event Action<ChatMessageArgs> EventHandler_ReceivedChatMessage;

    public static event Action<ServerMessage> EventHandler_ReceivedServerMessage;
    // In Game
    public static event Action<FighterState> EventHandler_ReceivedFighterState;
    // Dispatcher
    private void MsgDispatcher(MsgEnvelope _envelope)
    {
        switch (_envelope.op)
        {
            // Chat
            case ServerMsgTypes.Chat: 
                EventHandler_ReceivedChatMessage?.Invoke(Msg.DataAs<ChatMessageArgs>(_envelope)); 
                break;
            case ServerMsgTypes.Message:
                EventHandler_ReceivedServerMessage?.Invoke(Msg.DataAs<ServerMessage>(_envelope));
                break;
            // Lobby
            case ServerMsgTypes.RoomList:
                EventHandler_ReceivedRoomList?.Invoke(Msg.DataAs<RoomListArgs>(_envelope));
                break;
            case ServerMsgTypes.CreateRoom:
            case ServerMsgTypes.JoinRoom:
                EventHandler_JoinRoomResponse?.Invoke(Msg.DataAs<RequestResponse>(_envelope));
                break;
            // Room
            case ServerMsgTypes.RoomState:
                EventHandler_ReceivedRoomState?.Invoke(Msg.DataAs<RoomState>(_envelope));
                break;
            // In Game
            case ServerMsgTypes.FighterState:
                EventHandler_ReceivedFighterState?.Invoke(Msg.DataAs<FighterState>(_envelope));
                break;
            default: ; break;
        }
    }

    #endregion
    public void Chat(string _playerName, string _message)
    {
        var chatMsg = new ChatMessageArgs()
        {
            playerName = _playerName,
            message = _message
        };
        _ws.SendText(Msg.Wrap(ClientMsgTypes.Chat, chatMsg).ToJsonString());
    }

    public void RequestRoomList()
    {
        _ws.SendText(Msg.Wrap(ClientMsgTypes.RequestRoomList, new(){
        }).ToJsonString());
    }
}
