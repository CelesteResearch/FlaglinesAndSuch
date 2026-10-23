using System;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Randomness;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Everest.Content.Maps.Binary;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Binary.Source;
using Celeste.Graphics.Display;
using Celeste.Graphics.Rendering;
using Celeste.Graphics.Display.Colors;
using Celeste.Simulation.Presentation.Backdrops.Runtime;

namespace Celeste.Mods.FlaglinesAndSuch.Backdrops.Stars
{
    [CustomBackdrop("FlaglinesAndSuch/customDreamStars")]
    [RuntimeTypeAlias("FlaglinesAndSuch.customDreamStars")]
    public class CustomDreamStars : BackdropPresentation
    {
        public struct Stars
        {
            public Vector2 Position;
            public float Speed, Size;
        }

        private Stars[] stars;
        private readonly ISourceEngineContext engine;
        private Vector2 angle;
        private Vector2 lastCamera = Vector2.Zero;
        private float parallaxScroll;
        private string particleShape;
        public PackedColor StarColor;
        private ISourceRandom Random => ((ISourceRandomEnvironment)engine).Random;

        public CustomDreamStars(ISourceEngineContext engine, BinaryPackerElement child)
            : this(engine, child.ReadInstalledInt("count", 50), child.ReadInstalledFloat("minSpeed", 24f),
                child.ReadInstalledFloat("maxSpeed", 48f), child.ReadInstalledFloat("minSize", 2f),
                child.ReadInstalledFloat("maxSize", 8f), child.ReadInstalledString("Color", "008080"),
                child.ReadInstalledInt("AngleX", -2), child.ReadInstalledInt("AngleY", -7),
                child.ReadInstalledFloat("Scroll", 0f), child.ReadInstalledString("shape", ""))
        {
        }

        public CustomDreamStars(ISourceEngineContext engine, int count, float minSpeed, float maxSpeed, float minSize,
            float maxSize, string color, float angleX, float angleY, float scroll, string shape)
        {
            this.engine = engine;
            stars = new Stars[count];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].Position = new Vector2(SourceRandomMath.NextFloat(Random, 320f),
                    SourceRandomMath.NextFloat(Random, 180f));
                stars[i].Speed = minSpeed + SourceRandomMath.NextFloat(Random, maxSpeed - minSpeed);
                stars[i].Size = minSize + SourceRandomMath.NextFloat(Random, maxSize - minSize);
            }

            StarColor = SourceHexColor.HexToColor(color);
            angle = ((ISourceVectorEnvironment)engine).VectorArithmetic.Normalize(new Vector2(angleX, angleY));
            parallaxScroll = scroll;
            particleShape = shape;
        }

        public override void Update(ISourceBackdropScene scene)
        {
            base.Update(scene);
            if (!Visible) return;
            Vector2 camera = scene.Camera.Position;
            Vector2 delta = camera - lastCamera;
            for (int i = 0; i < stars.Length; i++)
                stars[i].Position += angle * stars[i].Speed * engine.Frame.DeltaTime - delta * parallaxScroll;
            lastCamera = camera;
        }

        public override void Render(ISourceRenderContext context, ISourceBackdropScene scene)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                Vector2 position = new Vector2(
                    ScalarMath.Mod(stars[i].Position.X, 320f + 2f * stars[i].Size) - stars[i].Size,
                    ScalarMath.Mod(stars[i].Position.Y, 180f + 2f * stars[i].Size) - stars[i].Size);
                switch (particleShape)
                {
                    case "Diamond":
                        SourcePrimitiveDrawing.Circle(context, null, position, stars[i].Size, StarColor, 1,
                            ((ISourceVectorEnvironment)engine).VectorArithmetic);
                        break;
                    case "Circle":
                        SourcePrimitiveDrawing.Circle(context, null, position, stars[i].Size, StarColor,
                            (int)Math.Log(stars[i].Size * 8f + 2f),
                            ((ISourceVectorEnvironment)engine).VectorArithmetic);
                        break;
                    case "FilledRect":
                        SourcePrimitiveDrawing.Rect(context, null, position, stars[i].Size, stars[i].Size, StarColor);
                        break;
                    default:
                        SourcePrimitiveDrawing.HollowRect(context, null, position, stars[i].Size, stars[i].Size,
                            StarColor);
                        break;
                }
            }
        }
    }
}
