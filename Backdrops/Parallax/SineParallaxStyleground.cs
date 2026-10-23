using System;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Rendering.States;
using Celeste.Core.Scheduling;
using Celeste.Everest.Content.Maps.Binary;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Binary.Source;
using Celeste.Game.Execution.Graphics;
using Celeste.Graphics.Content;
using Celeste.Graphics.Content.Atlas;
using Celeste.Graphics.Display;
using Celeste.Graphics.Display.Colors;
using Celeste.Simulation.Presentation.Backdrops.Runtime;

namespace Celeste.Mods.FlaglinesAndSuch.Backdrops.Parallax
{
    [CustomBackdrop("FlaglinesAndSuch/SineParallaxStyleground")]
    [RuntimeTypeAlias("FlaglinesAndSuch.SineParallaxStyleground")]
    public class SineParallaxStyleground : BackdropPresentation
    {
        public Vector2 CameraOffset = Vector2.Zero;
        public SourceBlendState BlendState = SourceBlendState.AlphaBlend;
        public AtlasEntry Texture;
        public float Alpha = 1f;
        public new PackedColor Color;
        private float fadeIn = 1f;
        public new bool InstantIn = true;
        public new bool InstantOut;
        private float sinValue, amplitude, frequency, xOffset, theta;
        private bool isVert;
        private Vector2 initialPos;
        private readonly ISourceEngineContext engine;

        public SineParallaxStyleground(ISourceEngineContext engine, BinaryPackerElement child)
        {
            this.engine = engine;
            bool extended = child.TryGetInstalledAttribute("Alpha", out _);
            string texture = child.ReadInstalledString("Texture", "");
            Initialize(child.ReadInstalledFloat("posX", 0), child.ReadInstalledFloat("posY", 0),
                child.ReadInstalledFloat("speedX", 0), child.ReadInstalledFloat("speedY", 0),
                child.ReadInstalledFloat("scrollX", 0), child.ReadInstalledFloat("scrollY", 0),
                child.ReadInstalledBool("loopX", false), child.ReadInstalledBool("loopY", false),
                child.ReadInstalledFloat("amplitude", 0), child.ReadInstalledFloat("frequency", 0),
                child.ReadInstalledFloat("offset", 0), child.ReadInstalledBool("SineVertically", false));
            if (extended)
                Configure(child.ReadInstalledFloat("Alpha", 0), child.ReadInstalledString("BlendMode", ""),
                    child.ReadInstalledString("Color", ""), child.ReadInstalledBool("InstantIn", false),
                    child.ReadInstalledBool("InstantOut", false), child.ReadInstalledBool("FlipX", false),
                    child.ReadInstalledBool("FlipY", false));
            else
            {
                Alpha = 1f;
                BlendState = SourceBlendState.AlphaBlend;
                Color = PackedColor.White;
            }

            Texture = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas[texture];
            Name = Texture.AtlasPath;
            initialPos = Position;
        }

        private void Initialize(float posX, float posY, float speedX, float speedY, float scrollX,
            float scrollY, bool loopX, bool loopY, float a, float f, float offset, bool vertical)
        {
            Position = new Vector2(posX, posY);
            Speed = new Vector2(speedX, speedY);
            Scroll = new Vector2(scrollX, scrollY);
            LoopY = loopY;
            LoopX = loopX;
            amplitude = a;
            frequency = f;
            xOffset = offset;
            isVert = vertical;
        }

        private void Configure(float alpha, string blendMode, string color, bool instantIn,
            bool instantOut, bool flipX, bool flipY)
        {
            Alpha = alpha;
            BlendState = blendMode == "Alphablend" ? SourceBlendState.AlphaBlend : SourceBlendState.Additive;
            Color = SourceHexColor.HexToColor(color);
            InstantIn = instantIn;
            InstantOut = instantOut;
            FlipX = flipX;
            FlipY = flipY;
        }

        public override void Update(ISourceBackdropScene scene)
        {
            sinValue = amplitude * (float)Math.Sin((double)(frequency * theta) + Math.PI * (double)xOffset);
            if (isVert) Position.Y = sinValue + initialPos.Y;
            else Position.X = sinValue + initialPos.X;
            base.Update(scene);
            Position += Speed * engine.Frame.DeltaTime;
            Position += WindMultiplier * scene.Wind * engine.Frame.DeltaTime;
            theta = (theta + engine.Frame.DeltaTime) % (float)(Math.PI * 2.0 / (double)frequency);
        }

        public override void Render(ISourceRenderContext context, ISourceBackdropScene scene)
        {
            Vector2 camera = Vector2Geometry.Floor(scene.Camera.Position + CameraOffset);
            Vector2 at = Vector2Geometry.Floor(Position - camera * Scroll);
            float alpha = fadeIn * Alpha * FadeAlphaMultiplier;
            if (FadeX != null) alpha *= FadeX.Value(camera.X + 160f);
            if (FadeY != null) alpha *= FadeY.Value(camera.Y + 90f);
            PackedColor color = Color;
            if (alpha < 1f) color *= alpha;
            if (color.A <= 1) return;
            if (LoopX)
            {
                while (at.X < 0f) at.X += Texture.FrameWidth;
                while (at.X > 0f) at.X -= Texture.FrameWidth;
            }

            if (LoopY)
            {
                while (at.Y < 0f) at.Y += Texture.FrameHeight;
                while (at.Y > 0f) at.Y -= Texture.FrameHeight;
            }

            bool flipX = false, flipY = false;
            if (FlipX && FlipY)
            {
                flipX = true;
                flipY = true;
            }
            else if (FlipX) flipX = true;
            else if (FlipY) flipY = true;

            for (float x = at.X; x < 320f; x += Texture.FrameWidth)
            {
                for (float y = at.Y; y < 180f; y += Texture.FrameHeight)
                {
                    var draw = new AtlasTextureDraw(Texture, new Vector2(x, y), Vector2.Zero,
                        color, Vector2.One, 0, flipX, flipY);
                    ((ISourceRenderCommandSink<AtlasTextureDraw>)context).Draw(in draw, null);
                    if (!LoopY) break;
                }

                if (!LoopX) break;
            }
        }
    }
}
