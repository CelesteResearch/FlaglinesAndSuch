using System;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Randomness;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Execution.Startup;
using Celeste.Game.Settings;
using Celeste.Graphics.Display.Colors;
using Celeste.Scene.Components.Oscillation;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Simulation.Presentation.PostProcessing.Bloom;

namespace Celeste.Mods.FlaglinesAndSuch.Entities.Lighting
{
    [CustomEntity("FlaglinesAndSuch/BonfireLight")]
    [RuntimeTypeAlias("FlaglinesAndSuch.BonfireLight")]
    public sealed class BonfireLight : SceneEntityState
    {
        public PackedColor LightColor;
        private readonly ISourceEngineContext engine;
        private readonly VertexLightState light;
        private readonly BloomPointState bloom;
        private readonly WigglerComponent wiggle;
        private readonly float randBase = 0.5f;
        private readonly float randAdd = 0.5f;
        private readonly float randFreq = 0.25f;
        private readonly bool disableOnPhotosensitive;
        private readonly bool fadeInOnEntry;
        private float brightness;
        private float multiplier;

        public BonfireLight(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset)
        {
            this.engine = engine;
            int fadeStart = 32;
            int fadeEnd = 64;
            float bloomRadius = 32f;
            float wiggleFrequency = 4f;
            float wiggleDuration = 0.2f;
            if (data.Has("brightnessVariance"))
            {
                fadeStart = data.Int("lightFadeStart");
                fadeEnd = data.Int("lightFadeEnd");
                bloomRadius = data.Float("bloomRadius");
                randBase = data.Float("baseBrightness");
                randAdd = data.Float("brightnessVariance");
                randFreq = data.Float("flashFrequency");
                wiggleFrequency = data.Float("wigglerFrequency");
                wiggleDuration = data.Float("wigglerDuration");
                disableOnPhotosensitive = data.Bool("photosensitivityConcern");
            }

            fadeInOnEntry = !data.Has("fadeIn") || data.Bool("fadeIn");
            LightColor = SourceHexColor.HexToColor(data.Attr("lightColor"));
            Tags = ((IOriginalGameTagsEnvironment)engine).Tags.TransitionUpdate.Value;
            Add(light = new VertexLightState(engine, Vector2.Zero, LightColor, 1f, fadeStart, fadeEnd));
            Add(bloom = new BloomPointState(Vector2.Zero, 1f, bloomRadius));
            Add(wiggle = ((ISourceWigglerPoolScope)engine).WigglerPool.Create(wiggleDuration, wiggleFrequency,
                value => light.Alpha = bloom.Alpha = Math.Min(1f, brightness + value * 0.25f) * multiplier));
        }

        public override void Awake(object scene)
        {
            base.Awake(scene);
            if ((!disableOnPhotosensitive || !((ISettingsContext)engine).Settings.DisableFlashes) &&
                !(randFreq <= 0f) && fadeInOnEntry)
                wiggle.Start();
        }

        public override void Update()
        {
            if (disableOnPhotosensitive && ((ISettingsContext)engine).Settings.DisableFlashes)
            {
                base.Update();
                return;
            }

            multiplier = fadeInOnEntry
                ? ScalarMath.Approach(multiplier, 1f, engine.DeltaTime * 2f,
                    ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
                : 1f;
            if (((ISourceScene)Scene).SourceScene.Clock.OnInterval(randFreq, engine))
            {
                brightness = randBase + SourceRandomMath.NextFloat(((ISourceRandomEnvironment)engine).Random, randAdd);
                wiggle.Start();
            }

            base.Update();
        }
    }
}
