using System;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Randomness;
using Celeste.Core.Scheduling;
using Celeste.Graphics.Rendering.Vertices;

namespace Celeste.Mods.FlaglinesAndSuch.Backdrops.Particles
{
    internal static class MovingParticleField
    {
        public static void Update<TParticle, TGeometry>(ISourceEngineContext engine, TParticle[] particles,
            TGeometry geometry, float target) where TParticle : struct, IMovingFieldParticle
            where TGeometry : struct, IMovingFieldGeometry
        {
            for (int i = 0; i < geometry.Count; i++)
            {
                particles[i].Position += particles[i].Direction * particles[i].Speed * engine.Frame.DeltaTime;
                float value = Vector2Geometry.Angle(particles[i].Direction);
                value = ScalarMath.AngleApproach(value, target,
                    particles[i].DirectionApproach * engine.Frame.DeltaTime);
                particles[i].Direction = Vector2Geometry.AngleToVector(value, 1f);
            }
        }

        public static void FillVertices<TParticle, TGeometry>(TParticle[] particles, TGeometry geometry,
            SourceVertexPositionColor[] vertices,
            int firstVertex, Vector2 camera) where TParticle : struct, IMovingFieldParticle
            where TGeometry : struct, IMovingFieldGeometry
        {
            for (int i = 0; i < geometry.Count; i++)
            {
                int index = firstVertex + i * 6;
                float length = ScalarMath.ClampedMap(geometry.GetShapeSpeed(particles[i].Speed), 0f, 1200f, 1f, 64f);
                float width = ScalarMath.ClampedMap(geometry.GetShapeSpeed(particles[i].Speed), 0f, 1200f, 3f, 0.6f);
                Vector2 direction = particles[i].Direction;
                Vector2 perpendicular = Vector2Geometry.Perpendicular(direction);
                Vector2 position = particles[i].Position;
                position.X = -32f + ScalarMath.Mod(position.X - camera.X * geometry.ScrollX, 384f);
                position.Y = -32f + ScalarMath.Mod(position.Y - camera.Y * geometry.ScrollY, 244f);
                Vector2 a = position - direction * length * 0.5f - perpendicular * width;
                Vector2 b = position + direction * length * 1f - perpendicular * width;
                Vector2 c = position + direction * length * 0.5f + perpendicular * width;
                Vector2 d = position - direction * length * 1f + perpendicular * width;
                PackedColor color = particles[i].Color * geometry.Alpha;
                vertices[index].Color = color;
                vertices[index].Position = new Vector3(a, 0f);
                vertices[index + 1].Color = color;
                vertices[index + 1].Position = new Vector3(b, 0f);
                vertices[index + 2].Color = color;
                vertices[index + 2].Position = new Vector3(c, 0f);
                vertices[index + 3].Color = color;
                vertices[index + 3].Position = new Vector3(a, 0f);
                vertices[index + 4].Color = color;
                vertices[index + 4].Position = new Vector3(c, 0f);
                vertices[index + 5].Color = color;
                vertices[index + 5].Position = new Vector3(d, 0f);
            }
        }
    }
}
