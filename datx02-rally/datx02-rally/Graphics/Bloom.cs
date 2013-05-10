using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace datx02_rally.Graphics
{
    public class Bloom
    {
        #region Fields and Properties
        private Game game;
        private GraphicsDevice device;
        private SpriteBatch spriteBatch;
        private GaussianBlur gaussianBlur;

        private Effect effect;
        private RenderTarget2D bloomTexture;
        private RenderTarget2D bloomCombinedTexture;

        /// <summary>
        /// Threshold for how much light is affected by the bloom.
        /// 1.0f means nothing will be affected, 0.0f means all light in the scene will be affected.
        /// </summary>
        public float Threshold { get; set; }

        /// <summary>
        /// Intensity for the bloom
        /// </summary>
        public float BloomIntensity { get; set; }

        /// <summary>
        /// Intensity for the original texture
        /// </summary>
        public float OriginalIntensity { get; set; }

        /// <summary>
        /// Saturation for the bloom, usually 1.0f
        /// </summary>
        public float BloomSaturation { get; set; }

        /// <summary>
        /// Saturation for the original texture, usually 1.0f
        /// </summary>
        public float OriginalSaturation { get; set; }
        #endregion

        public Bloom(Game game, GaussianBlur gaussianBlur)
        {
            this.game = game;
            this.device = game.GraphicsDevice;
            this.spriteBatch = (game as GameManager).spriteBatch;
            this.gaussianBlur = gaussianBlur;
            effect = game.Content.Load<Effect>(@"Effects\Bloom");

            bloomTexture = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height,
                false, SurfaceFormat.Color, DepthFormat.Depth24);
            bloomCombinedTexture = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height,
                false, SurfaceFormat.Color, DepthFormat.Depth24);

            Threshold = 0.3f;
            BloomIntensity = 1.4f;
            OriginalIntensity = 1.0f;
            BloomSaturation = 1.2f;
            OriginalSaturation = 1.2f;
        }

        public Texture2D PerformBloom(Texture2D srcTexture)
        {
            effect.Parameters["Threshold"].SetValue(Threshold);
            effect.Parameters["BloomIntensity"].SetValue(BloomIntensity);
            effect.Parameters["OriginalIntensity"].SetValue(OriginalIntensity);
            effect.Parameters["BloomSaturation"].SetValue(BloomSaturation);
            effect.Parameters["OriginalSaturation"].SetValue(OriginalSaturation);

            device.SetRenderTarget(bloomTexture);
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
            effect.CurrentTechnique = effect.Techniques["Bloom"];
            effect.CurrentTechnique.Passes[0].Apply();
            device.Clear(Color.Black);
            spriteBatch.Draw(srcTexture, Vector2.Zero, Color.White);
            spriteBatch.End();

            Texture2D blurredTexture = bloomTexture;
            if (gaussianBlur != null)
            {
                blurredTexture = gaussianBlur.PerformGaussianBlur(bloomTexture);
            }

            device.SetRenderTarget(bloomCombinedTexture);
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
            effect.CurrentTechnique = effect.Techniques["BloomCombine"];
            effect.CurrentTechnique.Passes[0].Apply();
            effect.Parameters["ColorMap"].SetValue(srcTexture);
            device.Clear(Color.Black);
            spriteBatch.Draw(blurredTexture, new Rectangle(0, 0, bloomTexture.Bounds.Width, bloomTexture.Bounds.Height), Color.White);
            spriteBatch.End();

            device.SetRenderTarget(null);

            return (Texture2D)bloomCombinedTexture;
        }
    }
}
