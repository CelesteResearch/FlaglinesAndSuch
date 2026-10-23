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

namespace Celeste.Mods.FlaglinesAndSuch.Triggers.Audio
{
    [CustomEntity("FlaglinesAndSuch/MusicIfFlag")]
    [RuntimeTypeAlias("FlaglinesAndSuch.MusicIfFlag")]
    public sealed class MusicIfFlag : SourceTriggerEntityState
    {
        private readonly ISourceEngineContext engine;
        String oldTrack;
        String Track;
        int Progress;
        bool ResetOnLeave;

        String flag;
        bool flagState;
        bool resetFlag;
        bool wasSuccesfullyTriggered;

        public MusicIfFlag(ISourceEngineContext engine, EntityData data, Vector2 offset) : base(
            ((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset,
            data.Width, data.Height, ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            this.engine = engine;
            Track = data.Attr("Track");
            Progress = data.Int("Progress");

            flag = data.Attr("Flag");
            flagState = data.Bool("Flag_State");
            resetFlag = data.Bool("Reset_Flag");
            ResetOnLeave = data.Bool("Reset_On_Leave");
        }

        public override void OnEnter(RoomPlayerSession player)
        {
            if ((Scene as OriginalLevelSceneRoot).Session.GetFlag(flag) != flagState)
            {
                return;
            }

            if (ResetOnLeave)
            {
                oldTrack = ((IOriginalGameplayProcess)engine).AudioSession.CurrentMusic;
            }

            ChapterCompletionSession session = (Scene as OriginalLevelSceneRoot).Session;
            session.Audio.Music.Event = ((IOriginalSfxEnvironment)engine).Sfx.EventnameByHandle(Track);
            if (Progress != 0)
            {
                session.Audio.Music.SetProgress(Progress);
            }

            OriginalAudioSession.Apply(session.Audio, ((IOriginalGameplayProcess)engine).AudioSession.ChannelOutput,
                ((IOriginalGameplayProcess)engine).AudioApplyPolicy);

            if (resetFlag)
            {
                (Scene as OriginalLevelSceneRoot).Session.SetFlag(flag, !flagState);
            }

            wasSuccesfullyTriggered = true;
        }

        public override void OnLeave(RoomPlayerSession player)
        {
            if (ResetOnLeave && wasSuccesfullyTriggered)
            {
                ChapterCompletionSession session = (Scene as OriginalLevelSceneRoot).Session;
                session.Audio.Music.Event = oldTrack;
                OriginalAudioSession.Apply(session.Audio, ((IOriginalGameplayProcess)engine).AudioSession.ChannelOutput,
                    ((IOriginalGameplayProcess)engine).AudioApplyPolicy);
            }

            wasSuccesfullyTriggered = false;
        }
    }
}
