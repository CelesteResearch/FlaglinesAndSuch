using Celeste.Core.Randomness;
using System;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Rendering;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Content.Maps.Binary;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Binary.Source;
using Celeste.Game.Execution.Graphics;
using Celeste.Graphics.Content;
using Celeste.Graphics.Display.Colors;
using Celeste.Graphics.Rendering.Primitives;
using Celeste.Graphics.Rendering.Vertices;
using Celeste.Simulation.Presentation.Backdrops.Runtime;

namespace Celeste.Mods.FlaglinesAndSuch.Backdrops.Particles
{
    [CustomBackdrop("FlaglinesAndSuch/customBossField")]
    [RuntimeTypeAlias("FlaglinesAndSuch.CustomBossField")]
    public class CustomBossField : BackdropPresentation
    {
        private readonly ISourceEngineContext engine;
        private MovingFieldParticle[] particles;
        private SourceVertexPositionColor[] BGverts, verts;
        public float Alpha = 1f;
        public int particleCount = 200;
        public PackedColor[] colors;
        public PackedColor BGcolor = PackedColor.Black;
        public int particlePosXmin, particlePosXmax = 384, particlePosYmin, particlePosYmax = 244;
        public Vector2 particleDir, particleDirB;
        public float ScrollX, ScrollY, squishFactor = 1f;

        public CustomBossField(ISourceEngineContext engine, BinaryPackerElement child)
            : this(engine, child.ReadInstalledString("bg_color", ""), child.ReadInstalledFloat("alpha", 0f),
                child.ReadInstalledInt("count", 0), child.ReadInstalledString("particle_colors", ""),
                child.ReadInstalledFloat("speed_min", 0f), child.ReadInstalledFloat("speed_max", 0f),
                child.ReadInstalledInt("pos_x_min", 0), child.ReadInstalledInt("pos_x_max", 0),
                child.ReadInstalledInt("pos_y_min", 0), child.ReadInstalledInt("pos_y_max", 0),
                child.ReadInstalledFloat("dir_x", 0f), child.ReadInstalledFloat("dir_y", 0f),
                child.ReadInstalledFloat("dir_x_alt", 0f), child.ReadInstalledFloat("dir_y_alt", 0f),
                child.ReadInstalledFloat("scroll_x", 0f), child.ReadInstalledFloat("scroll_y", 0f),
                child.ReadInstalledFloat("stretch_multiplier", 0f))
        {
        }

        public CustomBossField(ISourceEngineContext engine, string background, float alpha, int count,
            string particleColors, float minimumSpeed, float maximumSpeed, int minimumX, int maximumX,
            int minimumY, int maximumY, float directionX, float directionY, float alternateX, float alternateY,
            float scrollX, float scrollY, float stretch)
        {
            this.engine = engine;
            BGcolor = background.Length == 6 ? SourceHexColor.HexToColor(background) : PackedColor.Black * 0f;
            Alpha = alpha;
            string[] values = particleColors.Replace(" ", string.Empty).Split(',');
            colors = new PackedColor[values.Length];
            for (int i = 0; i < values.Length; i++) colors[i] = SourceHexColor.HexToColor(values[i]);
            particleCount = Math.Abs(count);
            particles = new MovingFieldParticle[particleCount];
            BGverts = new SourceVertexPositionColor[6];
            verts = new SourceVertexPositionColor[particleCount * 6];
            particlePosXmax = maximumX;
            particlePosXmin = Math.Min(minimumX, maximumX);
            particlePosYmax = maximumY;
            particlePosYmin = Math.Min(minimumY, maximumX);
            particleDir = new Vector2(directionX, directionY);
            particleDirB = new Vector2(alternateX, alternateY);
            ScrollX = scrollX;
            ScrollY = scrollY;
            if (stretch != 0f) squishFactor = stretch;
            UseSpritebatch = false;
            for (int i = 0; i < particleCount; i++)
            {
                particles[i].Speed = SourceRandomMath.Range(((ISourceRandomEnvironment)engine).Random, minimumSpeed,
                    maximumSpeed);
                particles[i].Direction = particleDir;
                particles[i].DirectionApproach =
                    SourceRandomMath.Range(((ISourceRandomEnvironment)engine).Random, .25f, 4f);
                particles[i].Position.X = SourceRandomMath.Range(((ISourceRandomEnvironment)engine).Random,
                    particlePosXmin, particlePosXmax);
                particles[i].Position.Y = SourceRandomMath.Range(((ISourceRandomEnvironment)engine).Random,
                    particlePosYmin, particlePosYmax);
                particles[i].Color = ((ISourceRandomEnvironment)engine).Random.Choose(colors);
            }
        }

        public override void Update(ISourceBackdropScene scene)
        {
            base.Update(scene);
            if (Visible && Alpha > 0f)
            {
                Vector2 direction = particleDir;
                if (scene.Bounds.Height > scene.Bounds.Width) direction = particleDirB;
                float angle = Vector2Geometry.Angle(direction);
                MovingParticleField.Update(engine, particles, new Geometry(this), angle);
            }
        }

        public override void Render(ISourceRenderContext context, ISourceBackdropScene scene)
        {
            float right = ((OriginalGameGraphicsResources)((ISourceGameGraphicsEnvironment)engine).GameGraphics).Buffers
                .Gameplay.Width + 10;
            float bottom = ((OriginalGameGraphicsResources)((ISourceGameGraphicsEnvironment)engine).GameGraphics)
                .Buffers.Gameplay.Height + 10;
            Vector2 camera = scene.Camera.Position;
            PackedColor color = BGcolor * Alpha;
            BGverts[0].Color = color;
            BGverts[0].Position = new Vector3(-10f, -10f, 0f);
            BGverts[1].Color = color;
            BGverts[1].Position = new Vector3(right, -10f, 0f);
            BGverts[2].Color = color;
            BGverts[2].Position = new Vector3(right, bottom, 0f);
            BGverts[3].Color = color;
            BGverts[3].Position = new Vector3(-10f, -10f, 0f);
            BGverts[4].Color = color;
            BGverts[4].Position = new Vector3(right, bottom, 0f);
            BGverts[5].Color = color;
            BGverts[5].Position = new Vector3(-10f, bottom, 0f);
            MovingParticleField.FillVertices(particles, new Geometry(this), verts, 0, camera);
            SourcePrimitiveGraphics.DrawVertices(((ISourcePrimitiveGraphicsEnvironment)engine).PrimitiveGraphics,
                Matrix4x4.Identity, BGverts, BGverts.Length);
            for (int x = 0;
                 x < ((OriginalGameGraphicsResources)((ISourceGameGraphicsEnvironment)engine).GameGraphics).Buffers
                 .Gameplay.Width + 64;
                 x += 384)
            for (int y = 0;
                 y < ((OriginalGameGraphicsResources)((ISourceGameGraphicsEnvironment)engine).GameGraphics).Buffers
                 .Gameplay.Height + 64;
                 y += 244)
                SourcePrimitiveGraphics.DrawVertices(((ISourcePrimitiveGraphicsEnvironment)engine).PrimitiveGraphics,
                    Matrix4x4.CreateTranslation(x, y, 0f), verts, verts.Length);
        }

        private readonly struct Geometry : IMovingFieldGeometry
        {
            private readonly CustomBossField owner;
            public Geometry(CustomBossField owner) => this.owner = owner;
            public int Count => owner.particleCount;
            public float GetShapeSpeed(float speed) => speed * owner.squishFactor;
            public float ScrollX => owner.ScrollX;
            public float ScrollY => owner.ScrollY;
            public float Alpha => owner.Alpha;
        }
    }
}
