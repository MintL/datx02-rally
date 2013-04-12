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
        // TODO: Remove, exists in GameComponent.Game
        public Game1 gameInstance;
        // TODO: Remove, GraphicsDevice exists in DrawableGameComponent.GraphicsDevice
        public GraphicsDeviceManager graphics;

        public ContentManager content;
        public readonly GameState gameState;
        protected SpriteBatch spriteBatch;
        public Rectangle Bounds { get; set; }
        public Vector2 Position { get; set; }

        public GameStateView(Game game, GameState gameState)
            : base(game)
        {
            this.gameInstance = game as Game1;
            graphics = gameInstance.Graphics;
            content = gameInstance.Content;
            this.gameState = gameState;
            spriteBatch = new SpriteBatch(game.GraphicsDevice);

            if (!(this is GamePlayView))
                Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Bounds = graphics.GraphicsDevice.Viewport.Bounds;
        }

        /// <summary>
        /// Updates the state.
        /// </summary>
        /// <returns>Next game state</returns>
        public abstract GameState UpdateState(GameTime gameTime);

        public abstract void ChangeResolution();

        public Vector2 GetScreenPosition(Vector2 position)
        {
            return position * new Vector2(Bounds.Width, Bounds.Height);
        }
    }
}
