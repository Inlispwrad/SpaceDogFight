namespace SpaceDogFight.Server.Game.Core;

public class PoizonZone
{
    private float Radius = 1000f;
    public float TimeElapsed = 0f;

    public void Update(float _deltaTime)
    {
        TimeElapsed += _deltaTime;
        Radius = ComputeRadius(TimeElapsed);
    }
    

    private float ComputeRadius(float _t)
    {
        if (_t < 60) return 1000f - _t * 2f;
        return Math.Max(100f, 800 - (_t - 60) * 5f);
    }

    public float GetRadius()
    {
        float unitSize = 32f;
        return Radius * unitSize;
    }
}