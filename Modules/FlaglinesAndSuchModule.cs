using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Runtime;

namespace Celeste.Mods.FlaglinesAndSuch.Modules
{
    public sealed partial class FlaglinesAndSuchModule(ISourceEngineContext engine) : SourceEverestModule(engine)
    {
        public override void Load()
        {
        }

        public override void Unload()
        {
            Celeste.Mods.FlaglinesAndSuch.Runtime.Sprites.FlaglinesSpriteResources.Unload(SourceEngine);
        }
    }
}
