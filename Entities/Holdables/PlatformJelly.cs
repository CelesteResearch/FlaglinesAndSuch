using Celeste.Simulation.Gameplay.Platforms.Collision;
using Celeste.Core.Reflection.Runtime;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Mods.FlaglinesAndSuch.Runtime.Sprites;
using Celeste.Simulation.Presentation.Particles.Definitions;
using Celeste.Core.Display;
using System;
using System.Collections;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Scheduling;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Graphics.Display;
using Celeste.Graphics.Content;
using Celeste.Graphics.Animation;
using Celeste.Simulation.Gameplay.Actors.Foundation;
using Celeste.Simulation.Gameplay.Holdables;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Interaction.Springs;
using Celeste.Simulation.Presentation.Hud.Tutorials.Birds;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Graphics;
using Celeste.Game.Audio.Runtime;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Formats.Map.Entities.Attributes;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Game.Gameplay.Mechanisms.DashSwitches;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Profiles;
using Celeste.Game.Execution.Startup;
using Celeste.Core.Rendering;
using Celeste.Scene.Components.Oscillation;
using Celeste.Simulation.Audio.Sources;
using Celeste.Simulation.Gameplay.Environment.Wind;
using Celeste.Simulation.Gameplay.Enemies.Seeker;
using Celeste.Input.Gameplay;
using Celeste.Input.Gameplay.Feedback;
using Celeste.Game.Controls.Source;
using Celeste.Game.Entities.Birds;
using Celeste.Game.Content.Dialogs;
using Celeste.Graphics.Rendering;

namespace Celeste.Mods.FlaglinesAndSuch.Entities.Holdables
{
    [CustomEntity("FlaglinesAndSuch/PlatformJelly")]
    [RuntimeTypeAlias("FlaglinesAndSuch.PlatformJelly")]
    public class PlatformJelly : SourceActorEntityState
    {
        private SourceJumpThruEntityState ridablePlatform;
        private Vector2 PlatformOffset = new Vector2(-12, -16), PlatformOffsetBig = new Vector2(-16, -16);
        private int platformSoundIndex = 40;
        private bool PermaNoGrab, HitboxWhenHeld, noTilt, biggerHitbox;
        public static ParticleTypePresentation P_Glide, P_GlideUp, P_Platform, P_Glow, P_Expand;

        public Vector2 Speed;

        public HoldableState Hold;

        private OriginalLevelSceneRoot level;
        private readonly ISourceEngineContext engine;

        private global::Celeste.Physics.Collision.SourceCollisionHandler onCollideH;

        private global::Celeste.Physics.Collision.SourceCollisionHandler onCollideV;

        private Vector2 prevLiftSpeed;

        private Vector2 startPos;

        private float noGravityTimer;

        private float highFrictionTimer;

        private bool bubble;

        private bool tutorial;

        private bool destroyed;

        private Sprite sprite;

        private WigglerComponent wiggler;

        private SineWaveState platformSine;

        private SoundSourceComponentState fallingSfx;

        private BirdTutorialGuiState tutorialGui;

        public PlatformJelly(Vector2 position, bool bubble, bool tutorial, int platformSound, bool noGrabbing,
            bool restLower, string overrideSprite, bool bigHitbox, bool oldHitbox, bool noRotation,
            ISourceEngineContext engine)
            : base(new PixelActor(null, position), engine,
                ((ISourcePlatformTargetMotionEnvironment)engine).TargetMotionPolicy)
        {
            this.engine = engine;
            this.bubble = bubble;
            this.tutorial = tutorial;
            startPos = Position;
            base.Collider = restLower
                ? new HitboxColliderGeometry(8, 8, -4, -16)
                : new HitboxColliderGeometry(8, 10, -4, -10);
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            OwnedComponents.Add(sprite =
                overrideSprite != ""
                    ? ((ISourceGameGraphicsEnvironment)engine).GameGraphics.SpriteFactory.Create(overrideSprite)
                    : FlaglinesSpriteResources.Create(engine, "PlatformJelly"));
            OwnedComponents.Add(wiggler = ((ISourceWigglerPoolScope)engine).WigglerPool.Create(0.25f, 4f));
            base.Depth = -5;
            OwnedComponents.Add(Hold = new HoldableState(engine, 0.3f));
            Hold.PickupCollider = new HitboxColliderGeometry(20f, 22f, -10f, -16f);
            Hold.SlowFall = true;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.SpeedGetter = () => Speed;
            Hold.OnHitSpring = value => HitSpring((ISourceSpringEntity)value);
            platformSine = new SineWaveState(engine, 0.3f);
            OwnedComponents.Add(platformSine);
            fallingSfx = ((ISourceSoundSourceEnvironment)engine).SoundSources.Create();
            OwnedComponents.Add(fallingSfx);
            OwnedComponents.Add(new WindMoverState(WindMode));
            platformSoundIndex = platformSound;
            PermaNoGrab = noGrabbing;
            HitboxWhenHeld = oldHitbox;
            biggerHitbox = bigHitbox;
            noTilt = noRotation;
        }

        public PlatformJelly(ISourceEngineContext engine, OriginalLevelEntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("bubble"), data.Bool("tutorial"), data.Int("platformSound"),
                data.Bool("noGrabbing"), data.Bool("restLower"), data.Attr("OverrideSprite", ""),
                data.Bool("bigHitbox"), data.Bool("NewHitboxBehavior"), data.Bool("noRotation"), engine)
        {
        }

        public static SceneEntityState Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data) => new PlatformJelly(level.EngineContext, data, offset);

        public override void Added(object scene)
        {
            base.Added(scene);
            level = (OriginalLevelSceneRoot)Scene;
            if (tutorial)
            {
                tutorialGui = new BirdTutorialGuiState(engine, ((IOriginalGameTagsEnvironment)engine).Tags.HUD.Value,
                    ((IOriginalBirdProcess)engine).BirdTutorialResources, this, new Vector2(0f, -24f),
                    ((IOriginalFancyTextProcess)engine).Text.Dialog.Clean("tutorial_carry"),
                    ((IOriginalFancyTextProcess)engine).Text.Dialog.Clean("tutorial_hold"), BirdTutorialButton.Grab);
                tutorialGui.Open = true;
                ((ISourceScene)Scene).SourceScene.QueueSourceEntity(tutorialGui);
            }

            Vector2 platformPosition = ExactPosition + (biggerHitbox ? PlatformOffsetBig : PlatformOffset);
            ridablePlatform = new SourceJumpThruEntityState(platformPosition, biggerHitbox ? 32 : 24, false,
                ((ISourcePlatformCollisionScene)level).SolidWorldFactory.CreateJumpThru(platformPosition, false));
            ((ISourceScene)scene).SourceScene.QueueSourceEntity(ridablePlatform);
            ridablePlatform.SurfaceSoundIndex = platformSoundIndex;
        }

        public override void Update()
        {
            if (ridablePlatform.Scene != null)
            {
                if (ridablePlatform.HasPlayerRider() || PermaNoGrab) Collidable = false;
                else if (!destroyed && !PermaNoGrab) Collidable = true;
                ridablePlatform.Collidable = !((!HitboxWhenHeld && Hold.IsHeld) ||
                                               (HitboxWhenHeld && Hold.Holder != null &&
                                                Hold.Holder.Sprite.CurrentAnimationID == "pickUp"));
            }

            if (((ISourceScene)Scene).SourceScene.Clock.OnInterval(0.05f, engine))
            {
                level.Particles.Emit(((IOriginalGameplayProcess)engine).ParticleTypes.Glider.P_Glow, 1,
                    base.Center + Vector2.UnitY * -9f, new Vector2(10f, 4f));
            }

            float target = ((!Hold.IsHeld)
                ? 0f
                : ((!Hold.Holder.OnGround())
                    ? ScalarMath.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, (float)Math.PI / 3f, -(float)Math.PI / 3f)
                    : ScalarMath.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, 0.6981317f, -0.6981317f)));
            if (!noTilt)
                sprite.Rotation = ScalarMath.Approach(sprite.Rotation, target, (float)Math.PI * engine.DeltaTime,
                    ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
            if (Hold.IsHeld && !Hold.Holder.OnGround() &&
                (sprite.CurrentAnimationID == "fall" || sprite.CurrentAnimationID == "fallLoop"))
            {
                if (!fallingSfx.Playing)
                {
                    PlaySound("event:/new_content/game/10_farewell/glider_engage", Position);
                    fallingSfx.Play("event:/new_content/game/10_farewell/glider_movement");
                }

                Vector2 speed = Hold.Holder.Speed;
                Vector2 vector = new Vector2(speed.X * 0.5f, (speed.Y < 0f) ? (speed.Y * 2f) : speed.Y);
                float value = ScalarMath.Map(((ISourceVectorEnvironment)engine).VectorArithmetic.Length(vector), 0f,
                    120f,
                    0f, 0.7f);
                fallingSfx.Param("glider_speed", value);
            }
            else
            {
                fallingSfx.Stop();
            }

            base.Update();
            if (!destroyed)
            {
                foreach (var actual in ((ISourceScene)Scene).SourceScene.Entities.GetTrackedEntities(
                             "Celeste.SeekerBarrier"))
                {
                    var entity = (SeekerBarrierState)actual;
                    entity.Collidable = true;
                    bool num = SourceEntityCollision.Check(this, entity);
                    entity.Collidable = false;
                    if (num)
                    {
                        destroyed = true;
                        Collidable = false;
                        if (Hold.IsHeld)
                        {
                            Vector2 speed2 = Hold.Holder.Speed;
                            Hold.Holder.Drop();
                            Speed = speed2 * 0.333f;
                            ((ISourceRumbleEnvironment)engine).RumbleSession.Rumble((int)RumbleStrength.Medium,
                                (int)RumbleLength.Medium);
                        }

                        OwnedComponents.Add(new SimulationCoroutine(engine, DestroyAnimationRoutine()));
                        return;
                    }
                }

                if (Hold.IsHeld)
                {
                    prevLiftSpeed = Vector2.Zero;
                }
                else if (!bubble)
                {
                    if (highFrictionTimer > 0f)
                    {
                        highFrictionTimer -= engine.DeltaTime;
                    }

                    GliderMotion.AdvanceSpeed(this, engine, Hold, ref Speed, ref prevLiftSpeed, ref noGravityTimer,
                        highFrictionTimer, ref level.Wind);

                    MoveH(Speed.X * engine.DeltaTime, onCollideH);
                    MoveV(Speed.Y * engine.DeltaTime, onCollideV);
                    if (base.Left < (float)level.Bounds.Left)
                    {
                        base.Left = level.Bounds.Left;
                        OnCollideH(new PixelCollision(-Vector2.UnitX, Vector2.Zero, Vector2.Zero));
                    }
                    else if (base.Right > (float)level.Bounds.Right)
                    {
                        base.Right = level.Bounds.Right;
                        OnCollideH(new PixelCollision(Vector2.UnitX, Vector2.Zero, Vector2.Zero));
                    }

                    if (base.Top < (float)level.Bounds.Top)
                    {
                        base.Top = level.Bounds.Top;
                        OnCollideV(new PixelCollision(-Vector2.UnitY, Vector2.Zero, Vector2.Zero));
                    }
                    else if (base.Top > (float)(level.Bounds.Bottom + 16))
                    {
                        ridablePlatform.DestroyStaticMovers();
                        ridablePlatform.RemoveSelf();
                        RemoveSelf();
                        return;
                    }

                    Hold.CheckAgainstColliders();
                }
                else
                {
                    Position = startPos + Vector2.UnitY * platformSine.Value * 1f;
                }

                Vector2 one = Vector2.One;
                if (!Hold.IsHeld)
                {
                    if (level.Wind.Y < 0f)
                    {
                        PlayOpen();
                    }
                    else
                    {
                        sprite.Play(PermaNoGrab ? "alt_idle" : "idle");
                    }
                }
                else if (Hold.Holder.Speed.Y > 20f || level.Wind.Y < 0f)
                {
                    if (level.SourceScene.Clock.OnInterval(0.04f, engine))
                    {
                        if (level.Wind.Y < 0f)
                        {
                            level.ParticlesBG.Emit(((IOriginalGameplayProcess)engine).ParticleTypes.Glider.P_GlideUp, 1,
                                Position - Vector2.UnitY * 20f, new Vector2(6f, 4f));
                        }
                        else
                        {
                            level.ParticlesBG.Emit(((IOriginalGameplayProcess)engine).ParticleTypes.Glider.P_Glide, 1,
                                Position - Vector2.UnitY * 10f, new Vector2(6f, 4f));
                        }
                    }

                    PlayOpen();
                    if (((ISourceGameInputEnvironment)engine).Input.GliderMoveY.Value > 0)
                    {
                        one.X = 0.7f;
                        one.Y = 1.4f;
                    }
                    else if (((ISourceGameInputEnvironment)engine).Input.GliderMoveY.Value < 0)
                    {
                        one.X = 1.2f;
                        one.Y = 0.8f;
                    }

                    ((ISourceRumbleEnvironment)engine).RumbleSession.Rumble((int)RumbleStrength.Climb,
                        (int)RumbleLength.Short);
                }
                else
                {
                    sprite.Play("held");
                }

                sprite.Scale.Y = ScalarMath.Approach(sprite.Scale.Y, one.Y, engine.DeltaTime * 2f,
                    ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
                sprite.Scale.X = ScalarMath.Approach(sprite.Scale.X, (float)Math.Sign(sprite.Scale.X) * one.X,
                    engine.DeltaTime * 2f, ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
                if (tutorialGui != null)
                {
                    tutorialGui.Open = tutorial && !Hold.IsHeld && (OnGround(4) || bubble);
                }
            }
            else
            {
                Position += Speed * engine.DeltaTime;
            }

            if (ridablePlatform.Scene != null)
                ridablePlatform.MoveTo(ExactPosition + (biggerHitbox ? PlatformOffsetBig : PlatformOffset));
        }

        private void PlayOpen()
        {
            if (sprite.CurrentAnimationID != "fall" && sprite.CurrentAnimationID != "fallLoop")
            {
                sprite.Play("fall");
                sprite.Scale = new Vector2(1.5f, 0.6f);
                level.Particles.Emit(((IOriginalGameplayProcess)engine).ParticleTypes.Glider.P_Expand, 16,
                    base.Center +
                    ((ISourceVectorEnvironment)engine).VectorArithmetic.Rotate(Vector2.UnitY * -12f, sprite.Rotation),
                    new Vector2(8f, 3f), -(float)Math.PI / 2f + sprite.Rotation);
                if (Hold.IsHeld)
                {
                    ((ISourceRumbleEnvironment)engine).RumbleSession.Rumble((int)RumbleStrength.Medium,
                        (int)RumbleLength.Short);
                }
            }
        }

        public override void Render(ISourceRenderContext context)
        {
            if (!destroyed)
            {
                sprite.DrawSimpleOutline(context);
            }

            base.Render(context);
            if (bubble)
            {
                for (int i = 0; i < 24; i++)
                {
                    SourcePrimitiveDrawing.Point(context, this, Position + PlatformAdd(i), PlatformColor(i));
                }
            }
        }

        private void WindMode(Vector2 wind)
        {
            if (!Hold.IsHeld)
            {
                if (wind.X != 0f)
                {
                    MoveH(wind.X * 0.5f);
                }

                if (wind.Y != 0f)
                {
                    MoveV(wind.Y);
                }
            }
        }

        private Vector2 PlatformAdd(int num)
        {
            return new Vector2(-12 + num,
                -5 + (int)Math.Round(Math.Sin(((ISourceScene)Scene).SourceScene.Clock.TimeActive + (float)num * 0.2f) *
                                     1.7999999523162842));
        }

        private PackedColor PlatformColor(int num)
        {
            if (num <= 1 || num >= 22)
            {
                return PackedColor.White * 0.4f;
            }

            return PackedColor.White * 0.8f;
        }

        private void OnCollideH(PixelCollision data)
        {
            if (data.Hit is OriginalDashSwitchState)
            {
                (data.Hit as OriginalDashSwitchState).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }

            if (Speed.X < 0f)
            {
                PlaySound("event:/new_content/game/10_farewell/glider_wallbounce_left", Position);
            }
            else
            {
                PlaySound("event:/new_content/game/10_farewell/glider_wallbounce_right", Position);
            }

            Speed.X *= -1f;
            sprite.Scale = new Vector2(0.8f, 1.2f);
        }

        private void OnCollideV(PixelCollision data)
        {
            if (Math.Abs(Speed.Y) > 8f)
            {
                sprite.Scale = new Vector2(1.2f, 0.8f);
                PlaySound("event:/new_content/game/10_farewell/glider_land", Position);
            }

            if (Speed.Y < 0f)
            {
                Speed.Y *= -0.5f;
            }
            else
            {
                Speed.Y = 0f;
            }
        }

        private void OnPickup()
        {
            if (bubble)
            {
                for (int i = 0; i < 24; i++)
                {
                    level.Particles.Emit(((IOriginalGameplayProcess)engine).ParticleTypes.Glider.P_Platform,
                        Position + PlatformAdd(i), PlatformColor(i));
                }
            }

            AllowPushing = false;
            Speed = Vector2.Zero;
            AddTag(((IOriginalGameTagsEnvironment)engine).Tags.Persistent.Value);
            ridablePlatform.AddTag(((IOriginalGameTagsEnvironment)engine).Tags.Persistent.Value);
            highFrictionTimer = 0.5f;
            bubble = false;
            wiggler.Start();
            tutorial = false;
        }

        private void OnRelease(Vector2 force)
        {
            if (force.X == 0f)
            {
                PlaySound("event:/new_content/char/madeline/glider_drop", Position);
            }

            AllowPushing = true;
            RemoveTag(((IOriginalGameTagsEnvironment)engine).Tags.Persistent.Value);
            ridablePlatform.RemoveTag(((IOriginalGameTagsEnvironment)engine).Tags.Persistent.Value);
            force.Y *= 0.5f;
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }

            Speed = force * 100f;
            wiggler.Start();
        }

        protected override void OnSquish(PixelCollision data)
        {
            if (!TrySquishWiggle(data))
            {
                ridablePlatform.DestroyStaticMovers();
                ridablePlatform.RemoveSelf();
                RemoveSelf();
            }
        }

        public bool HitSpring(ISourceSpringEntity spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == SpringOrientation.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    wiggler.Start();
                    return true;
                }

                if (spring.Orientation == SpringOrientation.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.Center.Y + 5f, 4f);
                    Speed.X = 160f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    wiggler.Start();
                    return true;
                }

                if (spring.Orientation == SpringOrientation.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.Center.Y + 5f, 4f);
                    Speed.X = -160f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    wiggler.Start();
                    return true;
                }
            }

            return false;
        }

        private IEnumerator DestroyAnimationRoutine()
        {
            PlaySound("event:/new_content/game/10_farewell/glider_emancipate", Position);
            sprite.Play("death");
            ridablePlatform.DestroyStaticMovers();
            ridablePlatform.RemoveSelf();
            yield return 1f;
            RemoveSelf();
        }

        private void PlaySound(string path, Vector2 position)
        {
            var process = (IOriginalGameplayProcess)engine;
            process.AudioSession.Play(path, position, process.SaveContext);
        }
    }
}
