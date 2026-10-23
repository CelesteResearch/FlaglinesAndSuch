using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Execution.Levels;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Triggers.Foundation;

namespace Celeste.Mods.FlaglinesAndSuch.Triggers.Flags
{
    [CustomEntity("FlaglinesAndSuch/FlagIfFlag")]
    [RuntimeTypeAlias("FlaglinesAndSuch.FlagIfFlag")]
    public sealed class FlagIfFlag : SourceTriggerEntityState
    {
        private readonly string ifFlag;
        private readonly string setFlag;
        private readonly bool ifState;
        private readonly bool setState;
        private readonly bool resetIfflag;
        private readonly bool onlyOnce;
        private bool triggered;

        public FlagIfFlag(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration,
                data.Position + offset, data.Width, data.Height,
                ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            ifFlag = data.Attr("if_flag");
            setFlag = data.Attr("set_flag");
            ifState = data.Bool("if_state");
            setState = data.Bool("set_state");
            resetIfflag = data.Bool("reset_if_flag");
            onlyOnce = data.Bool("only_once");
        }

        public override void OnEnter(RoomPlayerSession player)
        {
            if (triggered) return;
            if ((Scene as OriginalLevelSceneRoot).Session.GetFlag(ifFlag) != ifState) return;
            (Scene as OriginalLevelSceneRoot).Session.SetFlag(setFlag, setState);
            if (resetIfflag) (Scene as OriginalLevelSceneRoot).Session.SetFlag(ifFlag, !ifState);
            if (onlyOnce) triggered = true;
        }
    }
}
