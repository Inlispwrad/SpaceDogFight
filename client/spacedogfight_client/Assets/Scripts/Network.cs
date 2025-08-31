using Godot;
using System.Text;
using SpaceDogFight.Shared.Protocols;

public partial class Network : Node
{
    private WebSocketPeer _ws;
    // 按你实际端口改：如果是日志里 63623 就改成 63623；默认 Kestrel 本地是 5000
    private const string Url = "ws://localhost:63623/ws"; // or 63623

    private bool _connectedAnnounced = false;
    private double _heartbeat = 0;
    private double _reconnectCd = 0;
    private int _retry = 0;

    public override void _Ready()
    {
        
        
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
                }

                // 收包
                while (_ws.GetAvailablePacketCount() > 0)
                {
                    var pkt = _ws.GetPacket();
                    GD.Print("[Network] RECV: " + Encoding.UTF8.GetString(pkt));
                }

                // 心跳（每 15s）
                _heartbeat += delta;
                if (_heartbeat >= 15.0)
                {
                    _heartbeat = 0;
                    _ws.SendText("{\"type\":\"ping\"}");
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
    public void SendJson(string json)
    {
        if (_ws != null && _ws.GetReadyState() == WebSocketPeer.State.Open)
            _ws.SendText(json);
    }
}
