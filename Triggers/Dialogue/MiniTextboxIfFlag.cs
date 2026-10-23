using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Maps;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Randomness;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Attributes;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Execution.Levels;
using Celeste.Game.UI.Dialogue.MiniTextboxes;
using Celeste.Scene.Tracking;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Holdables;
using Celeste.Simulation.Gameplay.Triggers.Foundation;

namespace Celeste.Mods.FlaglinesAndSuch.Triggers.Dialogue
{
    [CustomEntity("FlaglinesAndSuch/MiniTextboxIfFlag")]
    [RuntimeTypeAlias("FlaglinesAndSuch.MiniTextboxIfFlag")]
    public sealed class MiniTextboxIfFlag : SourceTriggerEntityState
    {
        private enum Modes
        {
            OnPlayerEnter,
            OnLevelStart,
            OnTheoEnter
        }

        private readonly ISourceEngineContext engine;
        private readonly MapEntityId id;
        private readonly string[] dialogOptions;
        private readonly Modes mode;
        private bool triggered;
        private readonly bool onlyOnce;
        private readonly int deathCount;
        private readonly string flag;
        private readonly bool flagState;
        private readonly bool resetFlag;

        public MiniTextboxIfFlag(ISourceEngineContext engine, EntityData data, Vector2 offset, MapEntityId id)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration,
                data.Position + offset, data.Width, data.Height,
                ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            this.engine = engine;
            this.id = id;
            mode = EntityAttributeReader.Enum(data.Values, "mode", Modes.OnPlayerEnter);
            dialogOptions = data.Attr("dialog_id").Split(',');
            onlyOnce = data.Bool("only_once");
            deathCount = data.Int("death_count", -1);
            if (mode == Modes.OnTheoEnter) Add(new HoldableColliderState(delegate { Trigger(); }));
            flag = data.Attr("Flag");
            flagState = data.Bool("Flag_State");
            resetFlag = data.Bool("Reset_Flag");
        }

        public override void Awake(object scene)
        {
            base.Awake(scene);
            if (mode == Modes.OnLevelStart) Trigger();
        }

        public override void OnEnter(RoomPlayerSession player)
        {
            if (mode == Modes.OnPlayerEnter) Trigger();
        }

        private void Trigger()
        {
            if ((Scene as OriginalLevelSceneRoot).Session.GetFlag(flag) != flagState) return;
            if (!triggered && (deathCount < 0 ||
                               (Scene as OriginalLevelSceneRoot).Session.DeathsInCurrentLevel == deathCount))
            {
                triggered = true;
                if (resetFlag) (Scene as OriginalLevelSceneRoot).Session.SetFlag(flag, !flagState);
                ((ISourceScene)Scene).SourceScene.QueueSourceEntity(
                    new MiniTextboxState(((ISourceRandomEnvironment)engine).Random.Choose(dialogOptions), engine));
                if (onlyOnce) (Scene as OriginalLevelSceneRoot).Session.DoNotLoad.Add(id);
            }
        }
    }
}
