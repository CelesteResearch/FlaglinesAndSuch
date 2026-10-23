using System;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Randomness;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Everest.Content.Maps.Binary;
using Celeste.Formats.Map.Binary.Source;
using Celeste.Game.Execution.Graphics;
using Celeste.Graphics.Content;
using Celeste.Graphics.Display.Colors;
using Celeste.Simulation.Presentation.Backdrops.Godrays;
using Celeste.Simulation.Presentation.Backdrops.Runtime;

namespace Celeste.Mods.FlaglinesAndSuch.Backdrops.Light
{
    [CustomBackdrop("FlaglinesAndSuch/customGodrays")]
    [RuntimeTypeAlias("FlaglinesAndSuch.CustomGodrays")]
    public class CustomGodrays : GodraysBackdropPresentation
    {
        public int minWidth, maxWidth, minLength, maxLength;
        public float durationBase, durationAdd;
        public PackedColor rayColor, rayColorFade;
        public float scrollX, scrollY, speedX, speedY;
        public float angleX = -1.6707964f, angleY = 1f;
        public int RayCount = 6;
        private readonly bool extendedBounds;
        private ISourceRandom Random => ((ISourceRandomEnvironment)engine).Random;

        public CustomGodrays(ISourceEngineContext engine, BinaryPackerElement child)
            : this(engine, child.ReadInstalledInt("min_width", 0), child.ReadInstalledInt("max_width", 0),
                child.ReadInstalledInt("min_length", 0), child.ReadInstalledInt("max_length", 0),
                child.ReadInstalledFloat("Duration_base", 0f), child.ReadInstalledFloat("Duration_variance", 0f),
                child.ReadInstalledFloat("ray_color_alpha", 0f), child.ReadInstalledString("ray_color", "f52b63"),
                child.ReadInstalledString("fade_to_color", ""), child.ReadInstalledFloat("scroll_x", 0f),
                child.ReadInstalledFloat("scroll_y", 0f),
                child.ReadInstalledFloat("speed_x", 0f), child.ReadInstalledFloat("speed_y", 0f),
                child.ReadInstalledInt("ray_count", 0),
                child.ReadInstalledFloat("angle_x", 0f), child.ReadInstalledFloat("angle_y", 0f),
                child.ReadInstalledBool("extend_bounds", false))
        {
        }

        public CustomGodrays(ISourceEngineContext engine, int minwidth, int maxwidth, int minlength, int maxlength,
            float durationbase, float durationadd, float raycoloralpha, string raycolor, string raycolorfade,
            float scrollx, float scrolly, float speedx, float speedy, int raycount, float anglex, float angley,
            bool extendbounds)
            : base(engine, raycount, false)
        {
            maxWidth = maxwidth;
            minWidth = Math.Min(minwidth, maxwidth);
            maxLength = maxlength;
            minLength = Math.Min(minlength, maxlength);
            durationBase = durationbase;
            durationAdd = durationadd;
            rayColor = SourceHexColor.HexToColor(raycolor) * raycoloralpha;
            rayColorFade = raycolorfade == "" ? rayColor : SourceHexColor.HexToColor(raycolorfade) * raycoloralpha;
            scrollX = scrollx;
            scrollY = scrolly;
            speedX = speedx;
            speedY = speedy;
            RayCount = raycount;
            angleX = anglex;
            angleY = angley;
            extendedBounds = extendbounds;
            InitializeRays();
        }

        protected override void ResetRay(ref Ray ray, Vector2 wrapBounds)
        {
            ray.Percent = 0f;
            ray.Duration = Math.Abs(durationBase + SourceRandomMath.NextFloat(Random) * durationAdd);
            ray.Width = Random.Next(minWidth, maxWidth);
            ray.Length = Random.Next(minLength, maxLength);
            ray.X = SourceRandomMath.NextFloat(Random, wrapBounds.X + (extendedBounds ? ray.Width * 2f : 0f));
            ray.Y = SourceRandomMath.NextFloat(Random, wrapBounds.Y + (extendedBounds ? ray.Length * 2f : 0f));
        }

        protected override Vector2 GetWrapBounds(ISourceBackdropScene scene)
        {
            var gameplay = ((OriginalGameGraphicsResources)((ISourceGameGraphicsEnvironment)engine).GameGraphics)
                .Buffers.Gameplay;
            return new Vector2(gameplay.Width + 64, gameplay.Height + 64);
        }

        protected override GodrayMotionConfiguration GetMotionConfiguration()
            => new GodrayMotionConfiguration(Vector2Geometry.AngleToVector(angleX, angleY),
                new Vector2(scrollX, scrollY),
                speedY, rayColor, rayColorFade, extendedBounds, GodrayPositionRounding.Floor,
                GodrayGeometrySampling.BeforeReset, speedX);
    }
}
