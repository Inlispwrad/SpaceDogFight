using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace SpaceDogFight.Shared.Protocols;


public static class ClientMsgTypes
{
    // Client
    public const string CreateRoom = "client::create_room";
    public const string JoinRoom = "client::join_room";
    public const string LeaveRoom = "client::leave_room";
    
    public const string Chat = "client::chat";
}

public static class ServerMsgTypes
{
    // Server
    public const string JoinedRoom = "server::joined_room";
    public const string LeavedRoom = "server::leaved_room";
    public const string Chat = "server::chat";
    public const string RoomList = "server::room_list";
}

public sealed class MsgEnvelope(string _operator, JToken _jsonData)
{
    public string op { get; set; } = _operator;
    public int? seq { get; set; }
    public JToken data { get; set; } = _jsonData;
    
    public string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.Indented);
}

public static class Msg
{
    public static MsgEnvelope Wrap(string _op, object _data, int? _seq = null)
        => new MsgEnvelope(_op, _data is JToken jt ? jt : JToken.FromObject(_data) ) { seq = _seq, };

    public static byte[] ToBytes(MsgEnvelope _env)
        => System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_env));

    public static MsgEnvelope Parse(string _json)
        => JsonConvert.DeserializeObject<MsgEnvelope>(_json);

    public static T DataAs<T>(MsgEnvelope _env)
        => _env.data.ToObject<T>();
}