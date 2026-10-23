using System;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Maps;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Formats.Map.Entities.Attributes;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Execution.Startup;
using Celeste.Game.Audio;
using Celeste.Scene.Tracking;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Triggers.Foundation;
using Celeste.Simulation.Runtime.Session;

namespace Celeste.Mods.FlaglinesAndSuch.Triggers.Lighting
{
    [CustomEntity("FlaglinesAndSuch/FlagLightFade")]
    [RuntimeTypeAlias("FlaglinesAndSuch.FlagLightFade")]
    public sealed class FlagLightFade : SourceTriggerEntityState
    {
        private readonly ISourceEngineContext engine;
        public float LightAddFrom;

        public float LightAddTo;

        public PositionModes PositionMode;

        String flag;
        bool flagState;
        bool resetFlag;

        bool wasSuccesfullyTriggered;

        public FlagLightFade(ISourceEngineContext engine, EntityData data, Vector2 offset) : base(
            ((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset,
            data.Width, data.Height, ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            this.engine = engine;
            AddTag(((IOriginalGameTagsEnvironment)engine).Tags.TransitionUpdate.Value);
            LightAddFrom = data.Float("LightAddFrom");
            LightAddTo = data.Float("LightAddTo");
            PositionMode = data.Enum("positionMode", PositionModes.NoEffect);

            flag = data.Attr("Flag");
            flagState = data.Bool("Flag_state");
            resetFlag = data.Bool("Reset_Flag");
        }

        public override void OnEnter(RoomPlayerSession player)
        {
            base.OnEnter(player);
            if ((Scene as OriginalLevelSceneRoot).Session.GetFlag(flag) == flagState)
            {
                wasSuccesfullyTriggered = true;
            }
        }

        public override void OnStay(RoomPlayerSession player)
        {
            if (wasSuccesfullyTriggered)
            {
                OriginalLevelSceneRoot level = base.Scene as OriginalLevelSceneRoot;
                float num = level.Session.LightingAlphaAdd =
                    ((ISourceScalarEnvironment)engine).ScalarArithmetic.AddProduct(LightAddFrom,
                        LightAddTo - LightAddFrom, ScalarMath.Clamp(GetPositionLerp(player, PositionMode), 0f, 1f));
                level.Lighting.Alpha = level.BaseLightingAlpha + num;
            }
        }

        public override void OnLeave(RoomPlayerSession player)
        {
            base.OnLeave(player);
            if (resetFlag & wasSuccesfullyTriggered)
            {
                (Scene as OriginalLevelSceneRoot).Session.SetFlag(flag, !flagState);
            }

            wasSuccesfullyTriggered = false;
        }
    }
}
