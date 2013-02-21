using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally
{
    public class PrelightingRenderer
    {
        RenderTarget2D depthTarget;
        RenderTarget2D normalTarget;
        RenderTarget2D lightTarget;

        Effect depthNormalEffect;
        Effect lightingEffect;

        Model lightMesh;

    }
}
