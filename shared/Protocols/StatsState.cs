namespace Shared.Core.Protocols;

[Serializable]
public class StatsState
{
    public float hp { get; set; } // 生命值
    public float energy { get; set; } // 当前能量值
    public float energyRegen { get; set; } // 每秒能量恢复速度
}