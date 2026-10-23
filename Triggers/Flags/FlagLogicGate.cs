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

namespace Celeste.Mods.FlaglinesAndSuch.Triggers.Flags
{
    [CustomEntity("FlaglinesAndSuch/FlagLogicGate")]
    [RuntimeTypeAlias("FlaglinesAndSuch.FlagLogicGate")]
    public sealed class FlagLogicGate : SourceTriggerEntityState
    {
        private readonly ISourceEngineContext engine;
        String flag1;
        String flag2;
        String setFlag;


        bool setState;
        bool OnlyOnce;
        bool triggered;

        bool[] WorkableCases;

        public FlagLogicGate(ISourceEngineContext engine, EntityData data, Vector2 offset) : base(
            ((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset,
            data.Width, data.Height, ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            this.engine = engine;
            flag1 = data.Attr("flag1");
            flag2 = data.Attr("flag2");
            setFlag = data.Attr("set_flag");
            setState = data.Bool("set_state");
            OnlyOnce = data.Bool("only_once");
            WorkableCases = new bool[4];
            WorkableCases[0] = data.Bool("Case_0_0");
            WorkableCases[1] = data.Bool("Case_0_1");
            WorkableCases[2] = data.Bool("Case_1_0");
            WorkableCases[3] = data.Bool("Case_1_1");
        }

        public override void OnEnter(RoomPlayerSession player)
        {
            if (triggered)
            {
                return;
            }


            if (WorkableCases[
                    (Convert.ToInt32((Scene as OriginalLevelSceneRoot).Session.GetFlag(flag1)) * 2) +
                    Convert.ToInt32((Scene as OriginalLevelSceneRoot).Session.GetFlag(flag2))])
            {
                (Scene as OriginalLevelSceneRoot).Session.SetFlag(setFlag, setState);
            }

            if (OnlyOnce)
            {
                triggered = true;
            }
        }
    }
}
