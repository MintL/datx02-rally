using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Graphics
{
    /// <summary>
    /// Implemented with lots of help from
    /// http://www.dhpoware.com/demos/xnaGaussianBlur.html
    /// The comments in this file is taken from the above example.
    /// </summary>
    public class GaussianBlur
    {
        #region Fields
        private Game game;
        private GraphicsDevice device;
        private SpriteBatch spriteBatch;

        private Effect effect;
        private RenderTarget2D blurHTarget;
        private RenderTarget2D blurVTarget;

        private float[] kernel;
        private float sigma;
        private Vector2[] offsetsHoriz;
        private Vector2[] offsetsVert;

        int renderTargetWidth;
        int renderTargetHeight;

        /// <summary>
        /// Returns the radius of the Gaussian blur filter kernel in pixels.
        /// WARNING: This value must be equal to the radius value in GaussianBlur.fx.
        /// </summary>
        public int Radius { get; set; }

        /// <summary>
        /// Returns the blur amount. This value is used to calculate the
        /// Gaussian blur filter kernel's sigma value. Good values for this
        /// property are 2 and 3. 2 will give a more blurred result whilst 3
        /// will give a less blurred result with sharper details.
        /// </summary>
        public float Amount { get; set; }

        #endregion

        public GaussianBlur(Game game)
        {
            this.game = game;
            this.device = game.GraphicsDevice;
            this.spriteBatch = (game as GameManager).spriteBatch;

            effect = game.Content.Load<Effect>(@"Effects\GaussianBlur");

            // Since we're performing a Gaussian blur on a texture image the
            // render targets are half the size of the source texture image.
            // This will help improve the blurring effect.
            renderTargetWidth = device.Viewport.Width / 4;
            renderTargetHeight = device.Viewport.Height / 4;

            blurHTarget = new RenderTarget2D(device, renderTargetWidth, renderTargetHeight,
                false, SurfaceFormat.Color, DepthFormat.Depth24);
            blurVTarget = new RenderTarget2D(device, renderTargetWidth, renderTargetHeight,
                false, SurfaceFormat.Color, DepthFormat.Depth24);

            Radius = 7;
            Amount = 3.0f;

            ComputeKernel(Radius, Amount);
            ComputeOffsets(renderTargetWidth, renderTargetHeight);
        }

        /// <summary>
        /// Calculates the Gaussian blur filter kernel. This implementation is
        /// ported from the original Java code appearing in chapter 16 of
        /// "Filthy Rich Clients: Developing Animated and Graphical Effects for
        /// Desktop Java".
        /// </summary>
        /// <param name="blurRadius">The blur radius in pixels.</param>
        /// <param name="blurAmount">Used to calculate sigma.</param>
        private void ComputeKernel(int radius, float amount)
        {
            kernel = null;
            kernel = new float[radius * 2 + 1];
            sigma = radius / amount;

            float twoSigmaSquare = 2.0f * sigma * sigma;
            float sigmaRoot = (float)Math.Sqrt(twoSigmaSquare * Math.PI);
            float total = 0.0f;
            float distance = 0.0f;
            int index = 0;

            for (int i = -radius; i <= radius; ++i)
            {
                distance = i * i;
                index = i + radius;
                kernel[index] = (float)Math.Exp(-distance / twoSigmaSquare) / sigmaRoot;
                total += kernel[index];
            }

            for (int i = 0; i < kernel.Length; ++i)
                kernel[i] /= total;
        }

        /// <summary>
        /// Calculates the texture coordinate offsets corresponding to the
        /// calculated Gaussian blur filter kernel. Each of these offset values
        /// are added to the current pixel's texture coordinates in order to
        /// obtain the neighboring texture coordinates that are affected by the
        /// Gaussian blur filter kernel. This implementation has been adapted
        /// from chapter 17 of "Filthy Rich Clients: Developing Animated and
        /// Graphical Effects for Desktop Java".
        /// </summary>
        /// <param name="textureWidth">The texture width in pixels.</param>
        /// <param name="textureHeight">The texture height in pixels.</param>
        private void ComputeOffsets(float textureWidth, float textureHeight)
        {
            offsetsHoriz = null;
            offsetsHoriz = new Vector2[Radius * 2 + 1];

            offsetsVert = null;
            offsetsVert = new Vector2[Radius * 2 + 1];

            int index = 0;
            float xOffset = 1.0f / textureWidth;
            float yOffset = 1.0f / textureHeight;

            for (int i = -Radius; i <= Radius; ++i)
            {
                index = i + Radius;
                offsetsHoriz[index] = new Vector2(i * xOffset, 0.0f);
                offsetsVert[index] = new Vector2(0.0f, i * yOffset);
            }
        }

        public Texture2D PerformGaussianBlur(Texture2D srcTexture)
        {
            effect.CurrentTechnique = effect.Techniques[0];
            effect.Parameters["Weights"].SetValue(kernel);

            // Horizontal blur
            effect.Parameters["Offsets"].SetValue(offsetsHoriz);

            effect.CurrentTechnique.Passes[0].Apply();
            device.SetRenderTarget(blurHTarget);
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
            device.Clear(Color.Black);
            spriteBatch.Draw(srcTexture, new Rectangle(0, 0, renderTargetWidth, renderTargetHeight), Color.White);
            spriteBatch.End();

            //Vertical blur
            effect.Parameters["Offsets"].SetValue(offsetsVert);

            effect.CurrentTechnique.Passes[0].Apply();
            device.SetRenderTarget(blurVTarget);
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
            device.Clear(Color.Black);
            spriteBatch.Draw(blurHTarget, new Rectangle(0, 0, renderTargetWidth, renderTargetHeight), Color.White);
            spriteBatch.End();

            device.SetRenderTarget(null);
            return (Texture2D)blurVTarget;
        }
    }
}
