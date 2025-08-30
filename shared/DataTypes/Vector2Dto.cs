using System;

namespace Shared.Core.DataTypes;

[Serializable]
public sealed class Vector2Dto // Dto = Data Transfer Object
{
    public float x { get; set; } = 0;
    public float y { get; set; } = 0;
}