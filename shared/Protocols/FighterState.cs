using System;
using Newtonsoft.Json;
using Shared.Core.DataTypes;

namespace SpaceDogFight.Shared.Protocols;

[Serializable]
public sealed class FighterState
{
    // Stats Sync
    public int id { get; set; } // 战机唯一ID（可选：区分玩家1/玩家2）
    public float hp { get; set; } // 生命值
    public float energy { get; set; } // 当前能量值
    public float energyRegen { get; set; } // 每秒能量恢复速度
    
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