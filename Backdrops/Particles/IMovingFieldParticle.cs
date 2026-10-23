using System.Numerics;
using Celeste.Core.Display;

namespace Celeste.Mods.FlaglinesAndSuch.Backdrops.Particles
{
    internal interface IMovingFieldParticle
    {
        Vector2 Position { get; set; }
        Vector2 Direction { get; set; }
        float Speed { get; }
        PackedColor Color { get; }
        float DirectionApproach { get; }
    }
}
