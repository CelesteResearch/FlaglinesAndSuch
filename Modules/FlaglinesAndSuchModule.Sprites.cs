using Celeste.Mods.FlaglinesAndSuch.Runtime.Sprites;

namespace Celeste.Mods.FlaglinesAndSuch.Modules
{
    public sealed partial class FlaglinesAndSuchModule
    {
        public override void LoadContent(bool firstLoad) => FlaglinesSpriteResources.LoadContent(SourceEngine);
    }
}
