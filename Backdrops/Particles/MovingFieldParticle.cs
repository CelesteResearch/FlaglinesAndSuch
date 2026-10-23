using System.Numerics;
using Celeste.Core.Display;

namespace Celeste.Mods.FlaglinesAndSuch.Backdrops.Particles
{
    internal struct MovingFieldParticle : IMovingFieldParticle
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed;
        public PackedColor Color;
        public float DirectionApproach;

        Vector2 IMovingFieldParticle.Position
        {
            get => Position;
            set => Position = value;
        }

        Vector2 IMovingFieldParticle.Direction
        {
            get => Direction;
            set => Direction = value;
        }

        float IMovingFieldParticle.Speed => Speed;
        PackedColor IMovingFieldParticle.Color => Color;
        float IMovingFieldParticle.DirectionApproach => DirectionApproach;
    }
}
