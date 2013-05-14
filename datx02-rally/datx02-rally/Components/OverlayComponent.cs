using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using datx02_rally.Menus;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Components
{
    class OverlayComponent : DrawableGameComponent
    {

        public OverlayView Overlay { get; set; }
        public Texture2D OverlayTexture { get; set; }
        private SpriteBatch spriteBatch;

        public OverlayComponent(Game game, SpriteBatch spriteBatch)
            : base(game)
        {
            DrawOrder = 11;
            this.spriteBatch = spriteBatch;
        }

        public override void Draw(GameTime gameTime)
        {
            if (Overlay != null)
            {
                spriteBatch.Begin();
                Rectangle position = new Rectangle(GraphicsDevice.Viewport.Width / 2 - Overlay.RenderBounds.Width / 2,
                                                   GraphicsDevice.Viewport.Height / 2 - Overlay.RenderBounds.Height / 2,
                                                   Overlay.RenderBounds.Width, Overlay.RenderBounds.Height);
                spriteBatch.Draw(OverlayTexture, position, Color.White);
                spriteBatch.End();
            }
        }
    }
}
