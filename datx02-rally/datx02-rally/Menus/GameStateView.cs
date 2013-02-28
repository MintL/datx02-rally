using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Menus
{
    public abstract class GameStateView : DrawableGameComponent
    {
        public Game1 gameInstance;
        public GraphicsDeviceManager graphics;
        public ContentManager content;
        public readonly GameState gameState;
        protected SpriteBatch spriteBatch;
        public Rectangle Bounds { get; set; }

        public GameStateView(Game game, GameState gameState)
            : base(game)
        {
            this.gameInstance = game as Game1;
            graphics = gameInstance.Graphics;
            content = gameInstance.Content;
            this.gameState = gameState;
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            Initialize();
        }

        protected override void LoadContent()
        {
            Bounds = graphics.GraphicsDevice.Viewport.Bounds;
            base.LoadContent();
        }

        /// <summary>
        /// Updates the state.
        /// </summary>
        /// <returns>Next game state</returns>
        public abstract GameState UpdateState(GameTime gameTime);
    }
}
