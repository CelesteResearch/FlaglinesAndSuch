namespace Celeste.Mods.FlaglinesAndSuch.Backdrops.Particles
{
    internal interface IMovingFieldGeometry
    {
        int Count { get; }
        float GetShapeSpeed(float speed);
        float ScrollX { get; }
        float ScrollY { get; }
        float Alpha { get; }
    }
}
