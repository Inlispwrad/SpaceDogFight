using System;
using Newtonsoft.Json;
using Shared.Core.DataTypes;

namespace SpaceDogFight.Shared.Protocols;

[Serializable]
public class FighterState
{
    public int id { get; set; } // 战机唯一ID（可选：区分玩家1/玩家2）
    public float hp { get; set; } // 生命值
    public float energy { get; set; } // 当前能量值
    public float energyRegen { get; set; } // 每秒能量恢复速度
    public Vector2Dto position { get; set; } // 坐标 X, Y
    public float angle { get; set; } // 当前朝向角度（单位：度）
    public Vector2Dto velocity { get; set; } // 当前速度（矢量）
    public float angularSpeed { get; set; } // 转角速度（每秒角度变化量）

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }
}