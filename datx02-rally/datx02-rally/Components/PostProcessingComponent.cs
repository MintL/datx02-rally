using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using datx02_rally.Graphics;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally.Components
{
    class PostProcessingComponent : DrawableGameComponent
    {
        public RenderTarget2D RenderedImage { get; set; }

        Bloom bloom;
        GaussianBlur gaussianBlur;
        MotionBlur motionBlur;
        Matrix previousViewProjection;
        private bool motionBlurEnabled = true;

        public PostProcessingComponent(Game game)
            : base(game)
        {
            DrawOrder = 10; 
        }

        protected override void LoadContent()
        {
            gaussianBlur = new GaussianBlur(Game);
            bloom = new Bloom(Game, gaussianBlur);
            motionBlur = new MotionBlur(Game);
        }

        public override void Draw(GameTime gameTime)
        {
            // Apply bloom effect

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.White);

            var finalTexture = bloom.PerformBloom(RenderedImage);

            Matrix view = Game.GetService<CameraComponent>().View;

            if (Game.GetService<InputComponent>().GetKey(Keys.M))
            {
                motionBlurEnabled = !motionBlurEnabled;
            }

            var prelightingRenderer = Game.GetService<PrelightingRenderer>();

            if (motionBlurEnabled)
            {
                Matrix viewProjectionInverse = Matrix.Invert(view * prelightingRenderer.LightProjection);
                finalTexture = motionBlur.PerformMotionBlur(finalTexture, prelightingRenderer.DepthTarget, viewProjectionInverse, previousViewProjection);
                previousViewProjection = view * prelightingRenderer.LightProjection;
            }

            var spriteBatch = (Game as GameManager).spriteBatch;

            spriteBatch.Begin();
            spriteBatch.Draw(finalTexture, Vector2.Zero, Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
        }
    }
}
