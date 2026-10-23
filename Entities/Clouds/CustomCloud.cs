using System.Numerics;
using Celeste.Core.Mathematics.LinearAlgebra;
using Celeste.Core.Randomness;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Areas;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Graphics;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Gameplay.Platforms.Clouds;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Graphics.Display.Colors;
using Celeste.Scene.Components.Oscillation;
using Celeste.Scene.Entities;
using Celeste.Simulation.Audio.Sources;
using Celeste.Simulation.Gameplay.Platforms.Collision;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Simulation.Presentation.Particles.Definitions;

namespace Celeste.Mods.FlaglinesAndSuch.Entities.Clouds
{
    [CustomEntity("FlaglinesAndSuch/CustomCloud=Load")]
    [RuntimeTypeAlias("FlaglinesAndSuch.CustomCloud")]
    public sealed class CustomCloud : SourceJumpThruEntityState
    {
        private readonly ISourceEngineContext engine;
        private Sprite sprite;
        private WigglerComponent wiggler;
        private ParticleTypePresentation particleType;
        private SoundSourceComponentState sfx;
        private bool waiting = true;
        private float speed, startY, respawnTimer, timer;
        private bool returning, fragile, canRumble;
        private Vector2 scale;
        public bool? Small;
        public string OverrideText;

        public CustomCloud(ISourceEngineContext engine, EntityData data, Vector2 offset, ISourceSolidWorld world)
            : base(data.Position + offset, data.Width, false, world)
        {
            this.engine = engine;
            fragile = data.Bool("fragile");
            Small = data.Bool("small");
            startY = Y;
            Collider.Position.X = -16f;
            Collider.Width = 32f;
            timer = SourceRandomMath.NextFloat(((ISourceRandomEnvironment)engine).Random) * 4f;
            Add(wiggler = ((ISourceWigglerPoolScope)engine).WigglerPool.Create(.3f, 4f));
            SurfaceSoundIndex = 4;
            Add(new LightOccludeState(.2f));
            scale = Vector2.One;
            Add(sfx = new SoundSourceComponentState(engine));
            particleType = new ParticleTypePresentation(((IOriginalGameplayProcess)engine).ParticleTypes.Cloud.P_Cloud)
            {
                Color = SourceHexColor.HexToColor(fragile
                    ? data.Attr("FragileParticleColor", "")
                    : data.Attr("ParticleColor", ""))
            };
            OverrideText = data.Attr("OverrideSprite", "");
        }

        public static SceneEntityState Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data) => new CustomCloud(level.EngineContext, data, offset,
            ((ISourcePlatformCollisionScene)level).SolidWorldFactory.CreateJumpThru(data.Position + offset, false));

        public bool IsSmall(bool value) => Small ?? value;

        public override void Added(object scene)
        {
            base.Added(scene);
            string name = fragile ? "cloudFragile" : "cloud";
            if (IsSmall((byte)((IOriginalAreaKeyEnvironment)engine).AreaKeys
                    .GetSessionArea(((OriginalLevelSceneRoot)Scene).Session).Mode != 0))
            {
                Collider.Position.X += 2f;
                Collider.Width -= 6f;
                name += "Remix";
            }

            Add(sprite = ((OriginalGameGraphicsResources)((ISourceGameGraphicsEnvironment)engine).GameGraphics)
                .SpriteBank.Create(OverrideText != "" ? OverrideText : name));
            sprite.Origin = new Vector2(sprite.Width / 2f, 8f);
            sprite.OnFrameChange = animation =>
            {
                if (animation == "spawn" && sprite.CurrentAnimationFrame == 6) wiggler.Start();
            };
        }

        public override void Update()
        {
            base.Update();
            BouncingCloudMotion.Update(new MotionState(this));
        }

        public override void Render(ISourceRenderContext context)
        {
            Vector2 value = scale;
            value = SourceVector2Math.Multiply(value, 1f + .1f * wiggler.Value);
            sprite.Scale = value;
            base.Render(context);
        }

        private readonly struct MotionState : IBouncingCloudState
        {
            private readonly CustomCloud owner;
            public MotionState(CustomCloud owner) => this.owner = owner;
            public ISourceEngineContext Engine => owner.engine;
            public SourceJumpThruEntityState Platform => owner;
            public Sprite Sprite => owner.sprite;
            public SoundSourceComponentState Sound => owner.sfx;
            public ParticleTypePresentation ParticleType => owner.particleType;
            public bool Fragile => owner.fragile;
            public bool ShouldFade => owner.fragile && owner.Collidable && !owner.HasPlayerRider();
            public bool Pink => owner.fragile;
            public bool SquashWhileRising => owner.fragile;
            public ref Vector2 Scale => ref owner.scale;
            public ref float Timer => ref owner.timer;
            public ref float RespawnTimer => ref owner.respawnTimer;
            public ref float Speed => ref owner.speed;
            public ref float StartY => ref owner.startY;
            public ref bool Waiting => ref owner.waiting;
            public ref bool Returning => ref owner.returning;
            public ref bool CanRumble => ref owner.canRumble;

            public void BeginFade(bool respawn)
            {
                owner.Collidable = false;
                owner.sprite.Play("fade");
                if (respawn) owner.respawnTimer = 2.5f;
            }
        }
    }
}
