using System;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Everest.Content.Maps.Binary;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Binary.Source;
using Celeste.Graphics.Display.Colors;
using Celeste.Graphics.Rendering;
using Celeste.Graphics.Rendering.Drawing;
using Celeste.Simulation.Presentation.Backdrops.Runtime;

namespace Celeste.Mods.FlaglinesAndSuch.Backdrops.Grids
{
    [CustomBackdrop("FlaglinesAndSuch/VaporwaveGrid")]
    [RuntimeTypeAlias("FlaglinesAndSuch.VaporwaveGrid")]
    public class VaporwaveGrid : BackdropPresentation
    {
        private Vector2 lastCamera = Vector2.Zero;

        int Height;

        float botDistV;
        float botDistH;

        int linecountH;
        int linecountV;

        float speedV = 0.05f;
        float heightMod;

        float scrollTop = -0.2f;
        float scrollBot = -0.7f;

        public PackedColor mainColor;
        private readonly ISourceEngineContext engine;
        private int[] projectedHeights;
        private bool rendering;

        bool FLIP = false;
        int ViewX;
        int ViewY;

        private float[] TopXpts;

        public VaporwaveGrid(ISourceEngineContext engine, BinaryPackerElement child)
            : this(engine, child.ReadInstalledString("color", ""),
                child.ReadInstalledInt("Horizontal_lines", 0), child.ReadInstalledInt("Vertical_lines", 0),
                child.ReadInstalledFloat("Top_Scroll", 0f), child.ReadInstalledFloat("Bottom_Scroll", 0f),
                child.ReadInstalledInt("Top_Height", 0), child.ReadInstalledInt("view_X", 0),
                child.ReadInstalledInt("view_Y", 0), child.ReadInstalledBool("Flip_Y", false),
                child.ReadInstalledFloat("Speed_y", 0f))
        {
        }

        public VaporwaveGrid(ISourceEngineContext engine, string color, int Hlines, int Vlines, float scrollT,
            float scrollB, int height, int xview, int yview, bool flipit, float speedy)
        {
            this.engine = engine;
            mainColor = SourceHexColor.HexToColor(color);
            FLIP = flipit;
            Height = FLIP ? height : 180 - height;
            speedV = speedy;

            linecountH = Hlines;
            linecountV = Vlines;

            botDistH = Height / linecountH;
            botDistV = 320f / linecountV;

            TopXpts = new float[linecountV];
            for (int i = 0; i < TopXpts.Length; i++)
            {
                TopXpts[i] = height - ((float)(i) * botDistV);
            }

            scrollTop = -scrollT;
            scrollBot = -scrollB;

            ViewX = FLIP ? -xview : xview;
            ViewY = yview;
        }

        public override void Update(ISourceBackdropScene scene)
        {
            base.Update(scene);
            Vector2 position = scene.Camera.Position;
            Vector2 value = position - lastCamera;
            for (int i = 0; i < TopXpts.Length; i++)
            {
                TopXpts[i] += (value.X * scrollTop);
            }

            lastCamera = position;

            heightMod = (heightMod + speedV) % 1;
        }

        public override void Render(ISourceRenderContext context, ISourceBackdropScene scene)
        {
            bool previousRendering = rendering;
            rendering = true;
            try
            {
                SourcePrimitiveDrawing.Line((ISourceDrawScope)context, 0, Height, 320, Height, mainColor,
                    ((ISourceVectorEnvironment)engine).VectorArithmetic);
                int[] HlineHeights = getProjectedHeights(previousRendering);
                for (int i = 0; i < linecountH; i += 1)
                {
                    if (FLIP)
                    {
                        SourcePrimitiveDrawing.Line((ISourceDrawScope)context, 0, 180 + Height - HlineHeights[i], 320,
                            180 + Height - HlineHeights[i], mainColor,
                            ((ISourceVectorEnvironment)engine).VectorArithmetic);
                    }
                    else
                    {
                        SourcePrimitiveDrawing.Line((ISourceDrawScope)context, 0, HlineHeights[i], 320, HlineHeights[i],
                            mainColor, ((ISourceVectorEnvironment)engine).VectorArithmetic);
                    }
                }

                for (int i = 0; i < linecountV; i++)
                {
                    SourcePrimitiveDrawing.Line((ISourceDrawScope)context, findParallaxPos(TopXpts[i]), FLIP ? 0 : 180,
                        mod(TopXpts[i], 320), FLIP ? Height : Height + 1, mainColor,
                        ((ISourceVectorEnvironment)engine).VectorArithmetic);
                }
            }
            finally
            {
                rendering = previousRendering;
            }
        }

        private float mod(float x, float m)
        {
            return (x % m + m) % m;
        }

        float findParallaxPos(float X0)
        {
            return ((mod(X0, 320) - 160) * scrollBot / scrollTop) + 160;
        }

        private int[] getProjectedHeights(bool nested)
        {
            Vector2 viewpoint = new Vector2(ViewX, ViewY);

            float maxDistH = (-(FLIP ? Height : 180 - Height) * viewpoint.X) /
                             (viewpoint.Y - (FLIP ? Height : 180 - Height));

            float spacingH = maxDistH / linecountH;

            float HLineOffset = (heightMod * spacingH) % spacingH;

            int[] heights = nested ? new int[linecountH] : projectedHeights ??= new int[linecountH];

            for (int i = 0; i < linecountH; i++)
            {
                float realposX = HLineOffset + (i) * spacingH;
                double angle = Math.Asin(viewpoint.Y /
                                         Math.Sqrt(Math.Pow(realposX - viewpoint.X, 2) + Math.Pow(viewpoint.Y, 2)));
                heights[i] = FLIP
                    ? 180 + Height + (int)(Math.Tan(angle) * realposX)
                    : 180 - (int)(Math.Tan(angle) * realposX);
            }

            return heights;
        }
    }
}
