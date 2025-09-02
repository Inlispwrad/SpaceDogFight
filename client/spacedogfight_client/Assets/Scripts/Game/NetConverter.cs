using Godot;
using Shared.Core.DataTypes;

public static class NetConverters
{
    public static Vector2 ToGodot(this Vector2Dto v) => new(v.x, v.y);
    public static Vector2Dto ToDto(this Vector2 v) => new Vector2Dto { x = v.X, y = v.Y };
    public static float RadToDegF(this float r) => Mathf.RadToDeg(r);
    public static float DegToRadF(this float d) => Mathf.DegToRad(d);
}