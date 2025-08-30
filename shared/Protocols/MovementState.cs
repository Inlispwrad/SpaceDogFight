using System;
using Newtonsoft.Json;
using Shared.Core.DataTypes;

namespace SpaceDogFight.Shared.Protocols;

[Serializable]
public sealed class MovementState
{
    public Vector2Dto position { get; set; } // 坐标 X, Y
    public float angle { get; set; } // 当前朝向角度（单位：度）
    public Vector2Dto velocity { get; set; } // 当前速度（矢量）
    public float angularSpeed { get; set; } // 转角速度（每秒角度变化量）

    public string ToJson() => JsonConvert.SerializeObject(this);
    public static MovementState FromJson(string _json) => JsonConvert.DeserializeObject<MovementState>(_json);
}