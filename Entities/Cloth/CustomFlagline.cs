using System;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Components;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Gameplay.Environment.Ridge;
using Celeste.Graphics.Display.Colors;
using Celeste.Scene.Entities;

namespace Celeste.Mods.FlaglinesAndSuch.Entities.Cloth
{
    [RuntimeTypeAlias("FlaglinesAndSuch.CustomFlagline")]
    [CustomEntity("FlaglinesAndSuch/CustomFlagline")]
    public sealed class CustomFlagline : SceneEntityState
    {
        public PackedColor[] Colors;
        public PackedColor LineColor;
        public PackedColor PinColor;
        public int MinHeight;
        public int MaxHeight;
        public int MinLength;
        public int MaxLength;
        public int MinSpacing;
        public int MaxSpacing;
        public float FlagDroopAmount;

        public CustomFlagline(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration)
        {
            string[] colorStrings = data.Attr("FlagColors").Replace(" ", string.Empty).Split(',');
            Colors = new PackedColor[colorStrings.Length];
            for (int index = 0; index < colorStrings.Length; index++)
                Colors[index] = SourceHexColor.HexToColor(colorStrings[index]);
            LineColor = SourceHexColor.HexToColor(data.Attr("WireColor"));
            PinColor = SourceHexColor.HexToColor(data.Attr("PinColor"));
            MinHeight = data.Int("MinFlagHeight");
            MaxHeight = data.Int("MaxFlagHeight");
            MinLength = data.Int("MinFlagLength");
            MaxLength = data.Int("MaxFlagLength");
            MinSpacing = data.Int("MinSpacing");
            MaxSpacing = data.Int("MaxSpacing");
            FlagDroopAmount = data.Float("FlagDroopAmount");
            int entityDepth = data.Int("Depth", 8999);

            MinHeight = Math.Min(MinHeight, MaxHeight);
            MinLength = Math.Min(MinLength, MaxLength);
            MinSpacing = Math.Min(MinSpacing, MaxSpacing);
            if (MinSpacing <= 0 && Math.Abs(MinSpacing) >= Math.Abs(MinLength))
                MinSpacing = MinLength + 1;
            if (MinLength <= 0 && Math.Abs(MinLength) >= Math.Abs(MinSpacing))
                MinLength = MinSpacing + 1;
            if (MaxSpacing == 0)
                MaxSpacing = 1;

            Depth = entityDepth;
            Position = data.Position + offset;
            var flagline = ((IOriginalRidgeClothEnvironment)engine).Flaglines.Create(engine,
                data.Nodes[0] + offset, LineColor, PinColor, Colors, MinHeight, MaxHeight,
                MinLength, MaxLength, MinSpacing, MaxSpacing);
            Components.Add(flagline);
            flagline.ClothDroopAmount = FlagDroopAmount;
        }
    }
}
