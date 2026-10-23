using System.Runtime.CompilerServices;
using System.Xml;
using Celeste.Core.Scheduling;
using Celeste.Everest.Content.Registry;
using Celeste.Formats.Graphics.Sprites;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;

namespace Celeste.Mods.FlaglinesAndSuch.Runtime.Sprites
{
    internal sealed class FlaglinesSpriteResources
    {
        private static readonly ConditionalWeakTable<ISourceEngineContext, FlaglinesSpriteResources> Owners = new();
        private SpriteBank sprites;

        public static void LoadContent(ISourceEngineContext engine)
        {
            var xml = new XmlDocument();
            using (var stream = ((IEverestContentRegistryEnvironment)engine).SourceContentRegistry
                   .Get("FlaglinesAndSuch:/Graphics/FlaglinesAndSuch/CustomSprites").Stream) xml.Load(stream);
            Owners.GetValue(engine, _ => new FlaglinesSpriteResources()).sprites =
                new SpriteBank(engine, ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas, xml)
                    { XMLPath = "Graphics/FlaglinesAndSuch/CustomSprites.xml" };
        }

        public static Sprite Create(ISourceEngineContext engine, string name) =>
            Owners.GetValue(engine, _ => new FlaglinesSpriteResources()).sprites.Create(name);

        public static void Unload(ISourceEngineContext engine) => Owners.Remove(engine);
    }
}
