using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace datx02_rally.Menus
{
    public abstract class GameStateView : DrawableGameComponent
    {
        public Game1 gameInstance;
        public GraphicsDeviceManager graphics;
        public ContentManager content;

        public GameStateView(Game game) : base(game)
        {
            this.gameInstance = game as Game1;
            graphics = gameInstance.Graphics;
            content = gameInstance.Content;
            Initialize();
        }

        /// <summary>
        /// Updates the state.
        /// </summary>
        /// <returns>Next game state</returns>
        public abstract GameState UpdateState(GameTime gameTime);
    }
}
