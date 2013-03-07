using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Menus
{
    class MultiplayerMenu : GameStateView
    {
        bool connected = false;
        OverlayView connectOverlay;

        public MultiplayerMenu(Game game) : base(game, GameState.MultiplayerMenu) { }

        protected override void LoadContent()
        {
            connectOverlay = new OverlayView(gameInstance);
            connectOverlay.AddMenuItem(new ActionMenuItem("Connect", new ActionMenuItem.Action(ConnectToServer)));
            connectOverlay.Transparency = 0.2f;
            connectOverlay.Background = gameInstance.Content.Load<Texture2D>(@"Menu\Overlay_bg");
            connectOverlay.Bounds = new Rectangle(graphics.GraphicsDevice.Viewport.Bounds.Center.X - 250, graphics.GraphicsDevice.Viewport.Bounds.Center.Y - 250, 500, 500);

            base.LoadContent();
        }

        public override GameState UpdateState(GameTime gameTime)
        {
            if (!connected)
                connectOverlay.Draw(gameTime);
            return this.gameState;
        }

        public override void Draw(GameTime gameTime)
        {
            if (!connected)
                connectOverlay.Draw(gameTime);

            spriteBatch.Begin();
            string poo = string.Empty;
            for (int i = 0; i < 50; i++)
                 poo += "blalbalsdlsajdosajdoisajdoisajdiosajdoisajdoisajdoisajoisajdiosajdiosajoidjsaoidjsoaijdiosajdoisajdiosajdiosajdoisajdoisajoidjoisa\n";
            spriteBatch.DrawString(connectOverlay.MenuFont, poo, new Vector2(0, 0), Color.Blue);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void ConnectToServer()
        {
            Console.WriteLine("CONNECTTTTT");
        }
    }
}
