using SpaceDogFight.Shared.Protocols;

namespace SpaceDogFight.Server.Game.Net;

public class MsgContext
{
    public string PlayerId { get; init; } = "";
    public MsgEnvelope Envelope { get; init; } = null;
}