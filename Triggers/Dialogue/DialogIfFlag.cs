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

namespace Celeste.Mods.FlaglinesAndSuch.Triggers.Dialogue
{
    [CustomEntity("FlaglinesAndSuch/DialogIfFlag")]
    [RuntimeTypeAlias("FlaglinesAndSuch.DialogIfFlag")]
    public sealed class DialogIfFlag : SourceTriggerEntityState
    {
        private readonly ISourceEngineContext engine;


        private string dialogEntry;
        private bool triggered;
        private MapEntityId id;
        private bool onlyOnce;
        private bool endLevel;
        private int deathCount;

        String flag;
        bool flagState;
        bool resetFlag;

        public DialogIfFlag(ISourceEngineContext engine, EntityData data, Vector2 offset, MapEntityId entId)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset,
                data.Width, data.Height, ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            this.engine = engine;
            dialogEntry = data.Attr("dialogID");
            onlyOnce = data.Bool("onlyOnce", true);
            endLevel = data.Bool("endLevel", false);
            deathCount = data.Int("deathCount", -1);
            triggered = false;
            id = entId;

            flag = data.Attr("Flag");
            flagState = data.Bool("Flag_State");
            resetFlag = data.Bool("Reset_Flag");
        }

        public override void OnEnter(RoomPlayerSession player)
        {
            if (triggered || (Scene as OriginalLevelSceneRoot).Session.GetFlag("DoNotLoad" + id) ||
                (deathCount >= 0 && (Scene as OriginalLevelSceneRoot).Session.DeathsInCurrentLevel != deathCount))
            {
                return;
            }

            if ((Scene as OriginalLevelSceneRoot).Session.GetFlag(flag) != flagState)
            {
                return;
            }

            triggered = true;

            ((ISourceScene)Scene).SourceScene.QueueSourceEntity(
                new Celeste.Everest.Stories.Dialogue.EverestDialogCutsceneState(dialogEntry, player, endLevel, engine));

            if (resetFlag)
            {
                (Scene as OriginalLevelSceneRoot).Session.SetFlag(flag, !flagState);
            }

            if (onlyOnce)
                (Scene as OriginalLevelSceneRoot).Session.SetFlag("DoNotLoad" + id, true);
        }
    }
}
