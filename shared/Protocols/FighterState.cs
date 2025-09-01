using System;
using Newtonsoft.Json;
using Shared.Core.DataTypes;
using Shared.Core.Protocols;

namespace SpaceDogFight.Shared.Protocols;

[Serializable]
public sealed class FighterState
{
    public int playerName { get; set; } // 战机唯一ID（可选：区分玩家1/玩家2）
    
    // Stats Sync
    public StatsState? stats { get; set; } = null;
    // Movement Sync
    public MovementState? movement { get; set; } = null;

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static FighterState FromJson(string _json)
    {
        return JsonConvert.DeserializeObject<FighterState>(_json);
    }
}